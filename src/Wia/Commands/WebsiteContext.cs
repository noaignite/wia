using System.IO;
using System.Security.Principal;
using CommandLine;
using Wia.Model;

namespace Wia.Commands {
    public class WebsiteContext {
        public WebsiteContext() {
            SkipTasks = new string[] {};
        }

        [Option('d', "directory", HelpText = "Root project directory where solution file exists.")]
        public string CurrentDirectory { get; set; }

        [Option('n', "name", HelpText = "Name of project. Used in IIS.")]
        public string ProjectName { get; set; }

        [Option("webproject", HelpText = "Name of the web project directory.")]
        public string WebProjectName { get; set; }

        [Option('u', "url", HelpText = "Site domain to register in HOSTS.")]
        public string ProjectUrl { get; set; }

        [Option('v', "frameworkversion", HelpText = ".NET Framework version to use in IIS.")]
        public double FrameworkVersion { get; set; }

        [Option('e', "epiversion", HelpText = "EPiServer version, to know what license file is needed.")]
        public int EpiserverVersion { get; set; }

        [Option("poolmode", HelpText = "If AppPool should run in Integrated or Classic.", DefaultValue = AppPoolMode.Integrated)]
        public AppPoolMode AppPoolMode { get; set; }

        [Option("enable32bit", HelpText = "If app pool should set Enable 32 bit Applications.", DefaultValue = false)]
        public bool Enable32Bit { get; set; }

        [Option("skipHosts", HelpText = "Skip adding HOSTS entry.", DefaultValue = false)]
        public bool SkipHosts { get; set; }

        [OptionArray("skip", HelpText = "List of tasks to skip running (space separated).")]
        public string[] SkipTasks { get; set; }

        [Option('f', "force", HelpText = "Do not prompt to confirm configuration.", DefaultValue = false)]
        public bool Force { get; set; }

        public bool ExitAtNextCheck { get; set; }
        
        public string GetWebProjectDirectory() {
						if(WebProjectName==".") {
              return CurrentDirectory;
						} else {
							return Path.Combine(CurrentDirectory, WebProjectName);
						}
        }

        public bool HasAdministratorPrivileges() {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(id);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}