using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using CommandLine;
using InstallWebsite.Resolver;
using InstallWebsite.Utility;

namespace InstallWebsite {
    internal class Program {
        private static void Main(string[] args) {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-us");

            var context = new WebsiteContext();
            var requestedHelp = !Parser.Default.ParseArguments(args, context);

            if (requestedHelp) {
                return;
            }
            
            ContextResolver.ResolveContextDetails(context);

            if (context.ExitAtNextCheck)
                return;

            Console.WriteLine("\nConfiguration:\n");
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

        private static void DisplayContext(WebsiteContext context) {
            string[] ignoreProps = {"ExitAtNextCheck"};
            foreach (var prop in context.GetType().GetProperties()) {
                if (!ignoreProps.Contains(prop.Name)) {
                    Console.WriteLine("{0} = {1}", prop.Name, prop.GetValue(context, null));
                }
            }
        }

        private static void ProcessTasks(WebsiteContext context) {
            Console.WriteLine("\nInstallation started!\n");

            var tasks = GetTasksInAssembly();

            foreach (var task in tasks) {
                Console.WriteLine("Executing " + task.GetType().Name.Replace("Task", string.Empty) + " tasks...");

                Logger.TabIndention = 1;
                task.Execute(context);
                Logger.TabIndention = 0;
            }

            Logger.Log("");
            Logger.Success("Installation finished.");

            // *Check if program config exists
            // -Add url to hosts pointing to 127.0.0.1
            // Add new website in iis, path to web project
            // New app pool with .net 4.0, with credentials
            // Build site
            // Remove readlock on episerver framework
            // Copy a license file to web project
            // Open website url

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