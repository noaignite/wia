using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using InstallWebsite.Model;
using InstallWebsite.Utility;

namespace InstallWebsite.Tasks {
    internal class HostsTask : ITask {
        private const int ADMIN_REQUEST_CANCELED_BY_USER_ERROR_CODE = 1223;
#if DEBUG
        private const string ADD_HOSTS_ENTRY_FORMAT = @"echo. & echo {0}    {1} >> %systemdrive%\hosts.txt";
#else
        const string ADD_HOSTS_ENTRY_FORMAT = @"echo. & echo {0}    {1} >> %systemdrive%\windows\system32\drivers\etc\hosts";
#endif

        public IEnumerable<Type> DependsUpon() {
            return new[] {typeof (WebserverTask)};
        }

        public void Execute(WebsiteContext context) {
            if (context.SkipHosts) {
                Logger.Log("Will skip adding HOSTS entry.");
                return;
            }

            string host = new Uri(context.ProjectUrl).Authority;

            try {
                Logger.Log("Trying to add HOSTS entry for {0} to {1}...", host, "127.0.0.1");

                string addHostsCommand = string.Format(ADD_HOSTS_ENTRY_FORMAT, "127.0.0.1", host);

                ProcessStartInfo startInfo = new ProcessStartInfo {
                    FileName = @"cmd",
                    Verb = "runas",
                    Arguments = string.Format("cmd /K \"{0} & exit\"", addHostsCommand),
                };

                Process p = new Process {StartInfo = startInfo};
                p.Start();

                Logger.Log("Successfully added HOSTS entry.");
            }
            catch (Win32Exception ex) {
                if (ex.NativeErrorCode == ADMIN_REQUEST_CANCELED_BY_USER_ERROR_CODE) {
                    Logger.Warn("Failed to get administrator rights, installation will continue.");
                }
                else {
                    Logger.Error("Failed to add HOSTS entry. " + ex);
                }
            }
        }
    }
}