using System;
using System.IO;
using System.Security.Principal;

namespace InstallWebsite.Tasks
{
	class HostsTask : ITask
	{
		public void Execute(WebsiteContext context)
		{
			var domain = new Uri(context.ProjectUrl).Authority;
			
			// todo: separate this task to a new exe console, and open it from here with process.start with admin
			return;
			try
			{
				using (StreamWriter sw = File.AppendText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers/etc/hosts")))
				{
					sw.WriteLine("\n127.0.0.1\t{0}", domain);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Could not edit HOSTS file. " + ex.Message);
			}
		}
	}
}
