using System;
using System.Collections.Generic;
using System.IO;
using Wia.Commands;
using Wia.Model;
using Wia.Utility;

namespace Wia.Tasks {
    public class LicenseCopyTask : ITask {
        private const string LICENSE_FILENAME = "License.config";

        public IEnumerable<Type> DependsUpon() {
            return new[] {typeof (HostsTask)};
        }

        public void Execute(WebsiteContext context) {
            var licenseDirectory = Config.Instance.EpiserverLicensePath;
            var webProjectPath = context.GetWebProjectDirectory();
            var licensePath = Path.Combine(webProjectPath, LICENSE_FILENAME);

            if (File.Exists(licensePath)) {
                Logger.Warn("License file already exists.");
                return;
            }

            if (licenseDirectory.IsNullOrEmpty()) {
                Logger.Warn("Skipping because config is missing for EPiServer license directory.");
                Logger.Log(@"To update run command:");
                Logger.TabIndention += 1;
                Logger.Log(@"wia config license.directory c:\path\to\directory");
                Logger.TabIndention -= 1;
                return;
            }

            try {
                string licenseFileNeeded = Path.Combine(licenseDirectory, "CMS" + context.EpiserverVersion, LICENSE_FILENAME);

                if (!File.Exists(licenseFileNeeded)) {
                    Logger.Error("Required license file could not be found.");
                    Logger.Error("Path: " + licenseFileNeeded);
                    return;
                }

                File.Copy(licenseFileNeeded, licensePath);
            }
            catch (Exception ex) {
                Logger.Error("Failed to copy license file to website: " + ex.Message);
                return;
            }

            Logger.Success("Copied license file for EPiServer CMS {0} to website.", context.EpiserverVersion);
        }
    }
}