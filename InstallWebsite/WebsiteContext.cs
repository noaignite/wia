using System.Text;
using CommandLine;
using CommandLine.Text;

namespace InstallWebsite
{
	public class WebsiteContext : CommandLineOptionsBase
	{
		[Option("d", "directory", HelpText = "Root project directory where solution file exists.")]
		public string CurrentDirectory { get; set; }
		
		[Option("n", "name", HelpText = "Name for project.")]
		public string ProjectName { get; set; }

		[Option(null, "webproject", HelpText = "Name of the web project directory.")]
		public string WebProjectName { get; set; }

		[Option("u", "url", HelpText = "Site domain to register in HOSTS.")]
		public string ProjectUrl { get; set; }

		[Option("f", "frameworkversion", HelpText = ".NET Framework version to use in IIS.", DefaultValue = 4)]
		public double FrameworkVersion { get; set; }

		[Option("e", "epiversion", HelpText = "EPiServer version, to know what license file is needed.", DefaultValue = 6)]
		public int EpiserverVersion { get; set; }

		[Option(null, "poolmode", HelpText = "If app pool should run in Integrated (default) or Classic.", DefaultValue = AppPoolMode.Integrated)]
		public AppPoolMode AppPoolMode { get; set; }

		[Option(null, "enable32bit", HelpText = "If app pool should set Enable 32 bit Applications.", DefaultValue = false)]
		public bool Enable32Bit { get; set; }

		public bool ExitAtNextCheck { get; set; }

		[HelpOption]
		public string GetUsage()
		{
			return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
		}
	}
}