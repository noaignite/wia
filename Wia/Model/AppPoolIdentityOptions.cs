using CommandLine;

namespace Wia.Model {
    public class AppPoolIdentityOptions {
        [Option('u', "username", HelpText = "Username for the Application Pool Identity in form of \"domain\\username\".")]
        public string Username { get; set; }

        [Option('p', "password", HelpText = "Password for the Application Pool Identity (optional).")]
        public string Password { get; set; }

        [Option("reset", HelpText = "Removes the stored settings.")]
        public bool Reset { get; set; }

        public bool SuppliedLoginDetails { get { return !string.IsNullOrEmpty(Username) || !string.IsNullOrEmpty(Password); } }
    }
}
