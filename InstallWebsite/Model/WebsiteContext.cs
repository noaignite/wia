using System.Security.Principal;
using CommandLine;
using CommandLine.Text;

namespace InstallWebsite.Model {
    public class WebsiteContext {
        [Option('d', "directory", HelpText = "Root project directory where solution file exists.")]
        public string CurrentDirectory { get; set; }

        [Option('n', "name", HelpText = "Name for project.")]
        public string ProjectName { get; set; }

        [Option("webproject", HelpText = "Name of the web project directory.")]
        public string WebProjectName { get; set; }

        [Option('u', "url", HelpText = "Site domain to register in HOSTS.")]
        public string ProjectUrl { get; set; }

        [Option('v', "frameworkversion", HelpText = ".NET Framework version to use in IIS.")]
        public double FrameworkVersion { get; set; }

        [Option('e', "epiversion", HelpText = "EPiServer version, to know what license file is needed.")]
        public int EpiserverVersion { get; set; }

        [Option("poolmode", HelpText = "If app pool should run in Integrated (default) or Classic.", DefaultValue = AppPoolMode.Integrated)]
        public AppPoolMode AppPoolMode { get; set; }

        [Option("enable32bit", HelpText = "If app pool should set Enable 32 bit Applications.", DefaultValue = false)]
        public bool Enable32Bit { get; set; }

        [Option("skipHosts", HelpText = "Skip adding HOSTS entry.", DefaultValue = false)]
        public bool SkipHosts { get; set; }

        [Option('f', "force", HelpText = "Do not prompt to confirm configuration.", DefaultValue = false)]
        public bool Force { get; set; }

        public bool ExitAtNextCheck { get; set; }

        [HelpOption]
        public string GetUsage() {
            return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
        }

        public bool HasAdministratorPrivileges() {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(id);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}