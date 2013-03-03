using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using InstallWebsite.Model;
using InstallWebsite.Utility;

namespace InstallWebsite.Tasks {
    class HostsTask : ITask {
        public IEnumerable<Type> DependsUpon() { return new[] {typeof (EpiFrameworkUpdateTask)}; }

        public void Execute(WebsiteContext context) {
            if (context.SkipHosts) {
                Logger.Log("Will skip adding HOSTS entry.");
                return;
            }

            string host = new Uri(context.ProjectUrl).Authority;

            try {
                var hostsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"drivers\etc\hosts");

                var hostLines = File.ReadAllLines(hostsPath);
                var hostAlreadyAdded = hostLines.Any(h => h.Contains(host));
                
                if (hostAlreadyAdded) {
                    Logger.Warn("No change needed.");
                    return;
                }

                using (StreamWriter sw = File.AppendText(hostsPath)) {
                    string prefix = "";
                    
                    if (!string.IsNullOrWhiteSpace(hostLines.Last()))
                        prefix = "\n";

                    sw.Write("{1}127.0.0.1\t\t{0}", host, prefix);
                }
                Logger.Success("Successfully added HOSTS entry.");
            }
            catch (Exception ex)
			{
                Logger.Error("Failed to add HOSTS entry. " + ex);
			}
        }

        private const int ADMIN_REQUEST_CANCELED_BY_USER_ERROR_CODE = 1223;
        const string ADD_HOSTS_ENTRY_FORMAT = @"echo. & echo {0}    {1} >> %systemdrive%\windows\system32\drivers\etc\hosts";

        private void AddHostsInOtherProcess(string host) {
            try {
                Logger.Log("Trying to add HOSTS entry for {0} to {1}...", host, "127.0.0.1");

                string addHostsCommand = string.Format(ADD_HOSTS_ENTRY_FORMAT, "127.0.0.1", host);

                ProcessStartInfo startInfo = new ProcessStartInfo {
                    FileName = @"cmd",
                    Verb = "runas",
                    Arguments = string.Format("cmd /K \"{0} & exit\"", addHostsCommand),
                };

                Process p = new Process { StartInfo = startInfo };
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