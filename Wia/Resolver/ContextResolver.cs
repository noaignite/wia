using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Wia.Commands;
using Wia.Utility;

namespace Wia.Resolver {
    public class ContextResolver {
        public static void ResolveContextDetails(WebsiteContext context) {
            if (string.IsNullOrWhiteSpace(context.CurrentDirectory))
                context.CurrentDirectory = Environment.CurrentDirectory;
            else
                context.CurrentDirectory = Path.Combine(Environment.CurrentDirectory, context.CurrentDirectory);

            context.ProjectName = GetProjectName(context);
            context.WebProjectName = GetWebProjectName(context);
            context.ProjectUrl = GetProjectUrl(context);
            context.FrameworkVersion = GetFrameworkVersion(context);
            context.EpiserverVersion = GetEpiserverVersion(context);
        }

        private static double GetFrameworkVersion(WebsiteContext context) {
            if (context.ExitAtNextCheck)
                return -1;

            if (context.FrameworkVersion > 0)
                return context.FrameworkVersion;

            var webProjectFilePath = Directory.EnumerateFiles(context.GetWebProjectDirectory(), "*.csproj").FirstOrDefault();

            if (string.IsNullOrEmpty(webProjectFilePath) || !File.Exists(webProjectFilePath)) {
                context.ExitAtNextCheck = true;
                Console.WriteLine("The .csproj file for the \"{1}\" (web) project could not be found. Looked at: {0}",
                                  webProjectFilePath, context.WebProjectName);
                return -1;
            }

            var doc = XDocument.Load(webProjectFilePath);
            var targetFrameworkVersion = doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "TargetFrameworkVersion");

            if (targetFrameworkVersion == null) {
                context.ExitAtNextCheck = true;
                Console.WriteLine("Could not find TargetFrameworkVersion in Web project.");
                return -1;
            }

            var versionString = targetFrameworkVersion.Value.Replace("v", string.Empty);

            double version;

            if (!Double.TryParse(versionString, out version)) {
                context.ExitAtNextCheck = true;
                Console.WriteLine("Could not parse Framework Version from {0}.", versionString);
                return -1;
            }

            return version;
        }

        private static int GetEpiserverVersion(WebsiteContext context) {
            if (context.ExitAtNextCheck)
                return -1;

            if (context.EpiserverVersion > 0)
                return context.EpiserverVersion;

            var episerverDllFilePath = Directory.EnumerateFiles(context.CurrentDirectory, "EPiServer.dll", SearchOption.AllDirectories)
                                                .FirstOrDefault();

            if (episerverDllFilePath != null) {
                var version = FileVersionInfo.GetVersionInfo(episerverDllFilePath);
                return version.FileMajorPart;
            }

            return -1;
        }

        private static string GetWebProjectName(WebsiteContext context) {
            if (context.ExitAtNextCheck)
                return null;

            string webProjectName = context.WebProjectName;
            if (string.IsNullOrWhiteSpace(webProjectName)) {
                var webDirectories = Directory.EnumerateDirectories(context.CurrentDirectory)
                                              .Where(dir => Path.GetFileName(dir).Contains("Web"))
                                              .ToList();

                if (!webDirectories.Any()) {
                    context.ExitAtNextCheck = true;
                    Console.WriteLine("Web project could not be resolved. Please specifiy using \"--webproject Project.Web\". \n");
                    return null;
                }

                if (webDirectories.Count > 1) {
                    context.ExitAtNextCheck = true;
                    Console.WriteLine("You need to specificy which web project to use: \n--webproject " + 
                        webDirectories.Select(path => Path.GetFileName(path)).Aggregate((a, b) => a + "\n--webproject " + b) + "\n");
                    return null;
                }

                webProjectName = Path.GetFileName(webDirectories.FirstOrDefault());
            }

            var projectDirectory = Path.Combine(context.CurrentDirectory, webProjectName);

            if (!Directory.Exists(projectDirectory)) {
                context.ExitAtNextCheck = true;
                Console.WriteLine("Web project directory does not seem to exist at " + projectDirectory);
                return null;
            }

            return webProjectName;
        }

        private static string GetProjectName(WebsiteContext context) {
            if (context.ExitAtNextCheck)
                return null;

            string solutionFileName = Directory.EnumerateFiles(context.CurrentDirectory).FirstOrDefault(x => x.EndsWith(".sln"));
            string projectName = context.ProjectName;

            if (solutionFileName.IsNullOrEmpty()) {
                context.ExitAtNextCheck = true;
                Console.WriteLine("Could not find a solution file in the current directory.");
            }

            if (string.IsNullOrWhiteSpace(projectName)) {
                projectName = Path.GetFileNameWithoutExtension(solutionFileName);
            }

            return projectName;
        }

        private static string GetProjectUrl(WebsiteContext context) {
            if (context.ExitAtNextCheck)
                return null;

            var projectUrl = context.ProjectUrl;

            if (!string.IsNullOrWhiteSpace(projectUrl)) {
                if (!projectUrl.StartsWith("http://"))
                    projectUrl = "http://" + projectUrl;

                return projectUrl;
            }

            var webProjectFolderPath = context.GetWebProjectDirectory();
            var episerverConfigPath = Directory.GetFiles(webProjectFolderPath, "episerver.config", SearchOption.AllDirectories)
                                               .FirstOrDefault();

            if (!File.Exists(episerverConfigPath)) {
                // the episerver config section might be in web.config
                var webConfigPath = Path.Combine(webProjectFolderPath, "web.config");

                if (!File.Exists(webConfigPath)) {
                    // serious trouble if this file can not be found.
                    context.ExitAtNextCheck = true;
                    Console.WriteLine("The web.config file could not be found in " + webProjectFolderPath);
                    return null;
                }

                episerverConfigPath = webConfigPath;
            }

            var doc = XDocument.Load(episerverConfigPath);
            var siteSettings = doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "siteSettings");

            if (siteSettings == null) {
                context.ExitAtNextCheck = true;
                Console.WriteLine("Could not find the EPiServer configuration section in neither episerver.config or web.config.");
                return null;
            }

            projectUrl = siteSettings.Attribute("siteUrl").Value;
            return projectUrl;
        }
    }
}
