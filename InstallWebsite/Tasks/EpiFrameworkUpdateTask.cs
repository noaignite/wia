using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using InstallWebsite.Model;
using InstallWebsite.Properties;
using InstallWebsite.Utility;
using Microsoft.Web.Administration;

namespace InstallWebsite.Tasks {
    class EpiFrameworkUpdateTask : ITask {
        public IEnumerable<Type> DependsUpon() {
            return new[] { typeof(WebserverTask) };
        }

        public void Execute(WebsiteContext context) {
            using (ServerManager manager = new ServerManager()) {
                var webProjectDirectory = context.GetWebProjectDirectory();
                var site = manager.Sites.SingleOrDefault(s => s.Applications.Any(app => app.VirtualDirectories.Any(dir => dir.PhysicalPath == webProjectDirectory)));

                if (site == null) {
                    Logger.Warn("Could not find a matching site in IIS.");
                    return;
                }

                UpdateEpiserverFrameworkFile(context, site.Id);
            }
        }

        private void UpdateEpiserverFrameworkFile(WebsiteContext context, long siteId) {
            var episerverFrameworkFile = Directory.EnumerateFiles(context.GetWebProjectDirectory(), "EPiServerFramework.config", SearchOption.AllDirectories).FirstOrDefault();

            if (episerverFrameworkFile == null) {
                return;
            }

            var doc = XDocument.Load(episerverFrameworkFile);
            var automaticSiteMappingElement = doc.Descendants("automaticSiteMapping").FirstOrDefault();
            var key = string.Format("/LM/W3SVC/{0}/ROOT:{1}", siteId, Environment.MachineName);

            var alreadyUpdated = automaticSiteMappingElement.Descendants().Any(element => element.Attribute("key").Value.Equals(key));

            if (alreadyUpdated) {
                Logger.Warn("No change needed.");
                return;
            }

            var fileAttributes = File.GetAttributes(episerverFrameworkFile);
            var hadReadOnly = false;
            var epiSiteId = doc.Descendants("siteHosts").FirstOrDefault().Attribute("siteId").Value;

            if (IsReadOnly(fileAttributes)) {
                fileAttributes = RemoveAttribute(fileAttributes, FileAttributes.ReadOnly);
                File.SetAttributes(episerverFrameworkFile, fileAttributes);
                hadReadOnly = true;
            }

            // add new site mapping
            automaticSiteMappingElement.Add(new XElement("add", new XAttribute("key", key), new XAttribute("siteId", epiSiteId)));
            doc.Save(episerverFrameworkFile);

            if (hadReadOnly) {
                fileAttributes = SetAttribute(fileAttributes, FileAttributes.ReadOnly);
                File.SetAttributes(episerverFrameworkFile, fileAttributes);
            }

            Logger.Success("EPiServerFramework.config has been updated.");
            Logger.Success("Do not forget to update the file in source control.");
        }

        private bool IsReadOnly(FileAttributes fileAttributes) {
            return (fileAttributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;
        }

        private FileAttributes RemoveAttribute(FileAttributes fileAttributes, FileAttributes attributeToRemove) {
            return fileAttributes & ~attributeToRemove;
        }

        private FileAttributes SetAttribute(FileAttributes fileAttributes, FileAttributes attributeToSet) {
            return fileAttributes | attributeToSet;
        }

    }
}
