using Wia.Model;

namespace Wia {
    public class Config : ConfigBase {
        private static Config _instance;

        private Config() {}

        public static Config Instance {
            get { return _instance ?? (_instance = new Config()); }
        }

        [Setting("webserver", "username", HelpText = "Username for the Application Pool Identity in form of \"domain\\username\".")]
        public string AppPoolUsername { get; set; }

        [Setting("webserver", "password", HelpText = "Password for the Application Pool Identity (optional).")]
        public string AppPoolPassword { get; set; }

        [Setting("license", "directory", HelpText = "Absolute path to a directory with folders for each EPiServer CMS version containing a license file.")]
        public string EpiserverLicensePath { get; set; }
    }
}
