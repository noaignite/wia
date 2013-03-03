using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Xml.Linq;
using CommandLine;

namespace InstallWebsite
{
	class Program
	{
		static void Main(string[] args)
		{
			var context = new WebsiteContext();

			CommandLineParser.Default.ParseArguments(args, context);

			SetDefaultContextValues(context);

			if (!HasAdministratorPrivileges())
			{
				Console.WriteLine("Administrator permissions are needed to edit HOSTS file.");

				ProcessStartInfo startInfo = new ProcessStartInfo();
				startInfo.FileName = @"cmd";
				startInfo.Verb = "runas";
				startInfo.Arguments = "cmd /K \"cd \"" + context.CurrentDirectory + "\"";

				Process p = new Process();
				p.StartInfo = startInfo;
				p.Start();
			}


			if (context.ExitAtNextCheck)
				return;

			if (!HasAdministratorPrivileges())
				Console.WriteLine("Administrator permissions are needed to edit HOSTS file.");

			Console.WriteLine("\nConfiguration:\n");
			DisplayContext(context);

			Console.WriteLine("\nDo you want to continue installation with this configuration? (y/n)");
			var response = Console.ReadLine();

			if (response.Equals("y", StringComparison.InvariantCultureIgnoreCase)) {
				ProcessTasks(context);
			}
		}

		private static void DisplayContext(WebsiteContext context)
		{
			string[] ignoreProps = { "ExitAtNextCheck" };
			foreach (var prop in context.GetType().GetProperties())
			{
				if (!ignoreProps.Contains(prop.Name)) {
					Console.WriteLine("{0} = {1}", prop.Name, prop.GetValue(context, null));
				}
			}
		}

		private static bool HasAdministratorPrivileges()
		{
			WindowsIdentity id = WindowsIdentity.GetCurrent();
			WindowsPrincipal principal = new WindowsPrincipal(id);
			return principal.IsInRole(WindowsBuiltInRole.Administrator);
		}

		private static void SetDefaultContextValues(WebsiteContext context)
		{
			if (string.IsNullOrWhiteSpace(context.CurrentDirectory))
				context.CurrentDirectory = Environment.CurrentDirectory;

			context.WebProjectName = GetWebProjectName(context);
			context.ProjectName = GetProjectName(context);
			context.ProjectUrl = GetProjectUrl(context);
		}

		private static string GetWebProjectName(WebsiteContext context)
		{
			string webProjectName = context.WebProjectName;
			if (string.IsNullOrWhiteSpace(webProjectName))
			{
				var webDirectories = Directory.GetDirectories(context.CurrentDirectory).Where(d => Path.GetFileName(d).Contains("Web")).ToList();

				if (!webDirectories.Any())
				{
					context.ExitAtNextCheck = true;
					Console.WriteLine("Web project could not be resolved. Please specifiy using \"--webproject Project.Web\". \n");
					return null;
				}

				if (webDirectories.Count > 1)
				{
					context.ExitAtNextCheck = true;
					Console.WriteLine("You need to specificy which web project to use: \n--webproject " + webDirectories.Aggregate((a, b) => Path.GetFileName(a) + "\n--webproject " + Path.GetFileName(b)) + "\n");
					return null;
				}

				webProjectName = webDirectories.FirstOrDefault();
			}

			var projectDirectory = Path.Combine(context.CurrentDirectory, webProjectName);

			if (!Directory.Exists(projectDirectory))
			{
				context.ExitAtNextCheck = true;
				Console.WriteLine("Web project directory does not seem to exist at " + projectDirectory);
				return null;
			}

			return webProjectName;
		}

		private static string GetProjectName(WebsiteContext context)
		{
			if (context.ExitAtNextCheck)
				return null;

			string projectName = context.ProjectName;
			
			if (string.IsNullOrWhiteSpace(projectName))
			{
				var solutionFileName = Directory.GetFiles(context.CurrentDirectory).FirstOrDefault(x => x.EndsWith(".sln"));
				projectName = Path.GetFileNameWithoutExtension(solutionFileName);
			}

			return projectName;
		}

		private static string GetProjectUrl(WebsiteContext context)
		{
			if (context.ExitAtNextCheck)
				return null;
			
			var projectUrl = context.ProjectUrl;

			if (string.IsNullOrWhiteSpace(projectUrl)) {
				var episerverConfigPath = Path.Combine(context.CurrentDirectory, context.WebProjectName, "episerver.config");
			
				if (!File.Exists(episerverConfigPath))
				{
					context.ExitAtNextCheck = true;
					Console.WriteLine("Could not find episerver.config at " + episerverConfigPath);
					return null;
				}

				var doc = XDocument.Load(episerverConfigPath);
				var siteSettings = doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "siteSettings");

				if (siteSettings == null) {
					context.ExitAtNextCheck = true;
					Console.WriteLine("Could not find episerver.config at " + episerverConfigPath);
					return null;
				}
			
				projectUrl = siteSettings.Attribute("siteUrl").Value;
			}
			return projectUrl;
		}

		private static void ProcessTasks(WebsiteContext context)
		{
			Console.WriteLine("\nInstallation started!\n");

			var tasks = GetTasksInAssembly();
			
			foreach (var task in tasks)
			{
				task.Execute(context);
			}

			Console.WriteLine("Installation finished.");

			// *Check if program config exists
			// -Add url to hosts pointing to 127.0.0.1
			// Add new website in iis, path to web project
			// New app pool with .net 4.0, with credentials
			// Build site
			// Remove readlock on episerver framework
			// Copy a license file to web project
			// Open website url
		}

		private static IEnumerable<ITask> GetTasksInAssembly()
		{
			var type = typeof(ITask);
			var tasks = AppDomain.CurrentDomain.GetAssemblies().ToList()
				.SelectMany(s => s.GetTypes())
				.Where(t => type.IsAssignableFrom(t) && t.IsClass)
				.Select(Activator.CreateInstance)
				.OfType<ITask>();

			return tasks;
		}
	}
}
