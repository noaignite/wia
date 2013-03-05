using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Wia.Model;
using Wia.Utility;

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
    }
}
