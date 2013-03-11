using System;
using System.Collections.Generic;
using System.Net;
using Wia.Commands;
using Wia.Model;
using Wia.Utility;

namespace Wia.Tasks {
    class PingSiteTask : ITask {
        public IEnumerable<Type> DependsUpon() {
            return new[] { typeof(LicenseCopyTask) };
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
