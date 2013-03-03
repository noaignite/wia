using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using CommandLine.Text;

namespace InstallWebsite.Model {
    public class Options {
        [VerbOption("install", HelpText = "Install website.")]
        public WebsiteContext InstallWebsiteVerb { get; set; }

        [VerbOption("identity", HelpText = "Change the Application Pool Identity that is used in IIS.")]
        public AppPoolIdentityOptions AppPoolIdentityVerb { get; set; }

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
