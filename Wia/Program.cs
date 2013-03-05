using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using CommandLine;
using Wia.Model;
using Wia.Properties;
using Wia.Resolver;
using Wia.Utility;

namespace Wia {
    internal class Program {
        private static void Main(string[] args) {
            var prevColor = Console.ForegroundColor;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-us");
            Console.ForegroundColor = ConsoleColor.White;

            string invokedVerb = null;
            object invokedVerbOptions = null;
            
            var options = new Options();
            var requestedHelp = !Parser.Default.ParseArguments(args, options, (verb, subOptions) =>
            {
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
                case "identity":
                    InitiateIdentityTask((AppPoolIdentityOptions)invokedVerbOptions);
                    break;
            }

            Console.ForegroundColor = prevColor;
        }
        
        private static void InitiateInstallTask(WebsiteContext context) {
            if (!context.HasAdministratorPrivileges()) {
                Logger.Warn("WIA needs to run with administrator privileges to modify IIS and HOSTS-file.\nOpen new command prompt with \"Run as administrator\" and try again.");
                return;
            }
            
            ContextResolver.ResolveContextDetails(context);

            if (context.ExitAtNextCheck)
                return;

            Console.WriteLine("\nConfiguration:");
            DisplayContext(context);

            if (context.Force) {
                ProcessTasks(context);
            }
            else {
                Console.WriteLine("\nDo you want to continue installation with this configuration? (y/n)");
                var response = Console.ReadLine();

                if (response.Equals("y", StringComparison.InvariantCultureIgnoreCase)) {
                    ProcessTasks(context);
                }
            }
        }

        private static void InitiateIdentityTask(AppPoolIdentityOptions options) {
            if (options.Reset) {
                Settings.Default.IdentityUsername = null;
                Settings.Default.IdentityPassword = null;
                Settings.Default.Save();

                Logger.Success("Reseted the AppPool Identity settings.");
            } else if (options.SuppliedLoginDetails) {
                if (!string.IsNullOrEmpty(options.Username)) {
                    Settings.Default.IdentityUsername = options.Username;
                }
            
                if (!string.IsNullOrEmpty(options.Password)) {
                    Settings.Default.IdentityPassword = options.Password;
                }
                Settings.Default.Save();
                Logger.Success("Saved AppPool Identity settings.");
            }
            else {
                Logger.Log("Stored details:");
                
                Logger.Log("Username: " + Settings.Default.IdentityUsername);
                Logger.Log("Password: " + Settings.Default.IdentityPassword);
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
                Logger.Success("Installation finished successfully.");                
            }

            // * Add url to hosts pointing to 127.0.0.1
            // * Add new website in iis, path to web project
            // * New app pool with .net 4.0, with credentials
            // * Build site
            // * Remove readlock on episerver framework
            // * Ping website url
            // Copy a license file to web project

            // note: handle case when projectUrl = "localhost".
        }

        private static IEnumerable<ITask> GetTasksInAssembly() {
            var type = typeof (ITask);
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