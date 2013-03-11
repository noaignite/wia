using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CommandLine;
using Wia.Commands;
using Wia.Model;
using Wia.Resolver;
using Wia.Utility;

namespace Wia {
    internal class Program {
        [STAThread]
        private static void Main(string[] args) {
            var prevColor = Console.ForegroundColor;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-us");
            Console.ForegroundColor = ConsoleColor.White;
            
            string invokedVerb = null;
            object invokedVerbOptions = null;

            var options = new Options();
            var requestedHelp = !Parser.Default.ParseArguments(args, options, (verb, subOptions) => {
                invokedVerb = verb;
                invokedVerbOptions = subOptions;
            });

            if (requestedHelp) {
                Console.ForegroundColor = prevColor;
                return;
            }

            switch (invokedVerb) {
                case "install":
                    InitiateInstallTask((WebsiteContext)invokedVerbOptions);
                    break;
                case "config":
                    InitiateConfigTask((ConfigOptions)invokedVerbOptions);
                    break;
            }

            Console.ForegroundColor = prevColor;
        }

        private static void InitiateConfigTask(ConfigOptions options) {
            // Change config
            if (!options.ConfigKey.IsNullOrEmpty() && !options.ConfigValue.IsNullOrEmpty()) {
                if (!options.ConfigKey.Contains(".")) {
                    Logger.Log("Missing section prefix of config key.");
                    return;
                }

                var keyParts = options.ConfigKey.Split('.');
                var section = keyParts[0];
                var key = keyParts[1];

                Config.Instance.SaveValue(section, key, options.ConfigValue);
                Logger.Success("Config has been updated.");
                Logger.Log(options.ConfigKey + "=" + options.ConfigValue);
            }
            // Display config value
            else if (!options.ConfigKey.IsNullOrEmpty()) {
                if (!options.ConfigKey.Contains(".")) {
                    Logger.Log("Missing section prefix of config key.");
                    return;
                }

                var keyParts = options.ConfigKey.Split('.');
                var section = keyParts[0];
                var key = keyParts[1];
                string value;

                if (options.Reset) {
                    Config.Instance.SaveValue(section, key, null);
                }

                try {
                    value = Config.Instance.GetValue(section, key);
                }
                catch (KeyNotFoundException ex) {
                    Logger.Error(ex.Message);
                    return;
                }

                var configPair = Config.Instance.GetValues().First(opt => opt.Key.Equals(options.ConfigKey, StringComparison.OrdinalIgnoreCase));

                Logger.Log(configPair.HelpText);
                Logger.Space();
                Logger.Log(options.ConfigKey + "=" + value);

                Logger.Log("To update the value run:");
                Logger.Log("wia config " + options.ConfigKey + " valueHere");
            }
            // Display config list
            else {
                Logger.Log("Usage: wia config <key> [<value>] [--reset]");
                Logger.Space();

                foreach (var configPair in Config.Instance.GetValues()) {
                    Logger.Log(configPair.Key + "=" + configPair.Value + "");
                }
            }
        }

        private static void InitiateInstallTask(WebsiteContext context) {
            ContextResolver.ResolveContextDetails(context);

            if (context.ExitAtNextCheck)
                return;

            Console.WriteLine("\nConfiguration:");
            DisplayContext(context);

            if (!context.HasAdministratorPrivileges()) {
                Logger.Space();
                Logger.Warn("WIA needs to run with administrator privileges to modify IIS and HOSTS-file.\nOpen new command prompt with \"Run as administrator\" and try again.");
                return;
            }

            if (context.Force) {
                ProcessTasks(context);
            }
            else {
                Console.WriteLine("\nDo you want to continue installation with this configuration? (y/n)");
                var response = Console.ReadLine();

                if (response != null && response.Equals("y", StringComparison.InvariantCultureIgnoreCase)) {
                    ProcessTasks(context);
                }
            }
        }

        private static void DisplayContext(WebsiteContext context) {
            Logger.TabIndention = 1;

            string[] ignoreProps = { "CurrentDirectory", "ExitAtNextCheck", "AppPoolIdentityVerb", "Force", "SkipHosts", "SkipTasks" };
            foreach (var prop in context.GetType().GetProperties()) {
                if (!ignoreProps.Contains(prop.Name)) {
                    Logger.Log("{0} = {1}", prop.Name, prop.GetValue(context, null));
                }
            }

            Logger.TabIndention = 0;
        }

        private static void ProcessTasks(WebsiteContext context) {
            Console.WriteLine("\nInstallation started!\n");

            var tasks = GetTasksInAssembly();

            foreach (var task in tasks) {
                var taskName = task.GetType().Name.Replace("Task", string.Empty);

                if (context.SkipTasks.Any(t => t.Equals(taskName, StringComparison.OrdinalIgnoreCase)))
                    continue;

                Logger.Log(taskName + ":");

                Logger.TabIndention = 1;
                task.Execute(context);
                Logger.TabIndention = 0;

                if (context.ExitAtNextCheck) {
                    break;
                }
            }

            Logger.Space();

            if (context.ExitAtNextCheck) {
                Logger.Error("Installation failed.");
            }
            else {
                Logger.Success("Installation finished successfully (and URL has been copied to clipboard)");
                Clipboard.SetText(context.ProjectUrl);
            }
        }

        private static IEnumerable<ITask> GetTasksInAssembly() {
            var type = typeof(ITask);
            var tasks = AppDomain.CurrentDomain.GetAssemblies().ToList()
                                 .SelectMany(s => s.GetTypes())
                                 .Where(t => type.IsAssignableFrom(t) && t.IsClass)
                                 .Select(Activator.CreateInstance)
                                 .OfType<ITask>()
                                 .ToList();

            return tasks.DependencySort(task => GetDependantTasks(tasks, task));
        }

        private static IEnumerable<ITask> GetDependantTasks(IEnumerable<ITask> tasks, ITask task) {
            var dependsUpon = task.DependsUpon();

            if (dependsUpon == null) {
                return new List<ITask>();
            }

            var dependsUponList = dependsUpon as List<Type> ?? dependsUpon.ToList();
            return tasks.Where(t => dependsUponList.Contains(t.GetType()));
        }
    }
}