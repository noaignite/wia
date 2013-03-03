using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using InstallWebsite.Model;
using InstallWebsite.Utility;

namespace InstallWebsite.Tasks {
    class PingSiteTask : ITask {
        public IEnumerable<Type> DependsUpon() {
            return new[] { typeof(BuildTask) };
        }

        public void Execute(WebsiteContext context) {
            var projectUri = new Uri(context.ProjectUrl.ToLower());
            Logger.Log("Requesting {0}...", projectUri.Host);

            try {
                WebClient client = new WebClient();
                client.DownloadStringAsync(projectUri);
                Logger.Success("Site is being requested in the background.");
            }
            catch (Exception ex) {
                Logger.Error("Failed to ping site. " + ex.Message);
            }
        }
    }
}
