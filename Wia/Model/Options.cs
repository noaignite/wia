using CommandLine;
using CommandLine.Text;

namespace Wia.Model {
    public class Options {
        [VerbOption("install", HelpText = "Install website.")]
        public WebsiteContext InstallWebsiteVerb { get; set; }

        [VerbOption("config", HelpText = "Change configuration settings for WIA.")]
        public ConfigOptions ConfigVerb { get; set; }

        [HelpOption]
        public string GetUsage() {
            return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
        }

        [HelpVerbOption]
        public string GetUsage(string verb) {
            return HelpText.AutoBuild(this, verb);
        }
    }
}
