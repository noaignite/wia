using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Wia.Commands;
using Wia.Utility;
using ITask = Wia.Model.ITask;

namespace Wia.Tasks {
    class BuildTask : ITask {
        public IEnumerable<Type> DependsUpon() {
            return new [] { typeof(HostsTask) };
        }

        public void Execute(WebsiteContext context) {
            Logger.Log("Building the solution...");
            
            var solutionFilePath = Directory.GetFiles(context.CurrentDirectory).FirstOrDefault(x => x.EndsWith(".sln"));
            var properties = new Dictionary<string, string> {
                {"Configuration", "Debug"}
            };
            
            var buildParameters = new BuildParameters();
            var buildLoggger = new InMemoryBuildLogger();
            buildParameters.Loggers = new[] {buildLoggger};
            var buildRequest = new BuildRequestData(solutionFilePath, properties, null, new[] { "Build" }, null);
            var buildResult = BuildManager.DefaultBuildManager.Build(buildParameters, buildRequest);
            
            if (buildResult.OverallResult == BuildResultCode.Failure) {
                Logger.Error("Failed to build the solution!");
                Logger.Space();
                
                foreach (var buildError in buildLoggger.BuildErrors) {
                    Logger.Error(buildError);
                }
                Logger.Space();
            }
            else
                Logger.Success("Solution successfully built.");
        }

        class InMemoryBuildLogger : ILogger {
            public List<string> BuildErrors { get; private set; }

            public void Initialize(IEventSource eventSource) {
                BuildErrors = new List<string>();
                eventSource.ErrorRaised += EventSourceOnErrorRaised;
            }

            private void EventSourceOnErrorRaised(object sender, BuildErrorEventArgs buildErrorEventArgs) {
                BuildErrors.Add(buildErrorEventArgs.Message);
            }

            public void Shutdown() { }
            public LoggerVerbosity Verbosity { get; set; }
            public string Parameters { get; set; }
        }
    }
}
