using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InstallWebsite.Tasks {
    public class HelloWorldTask : ITask {
        public IEnumerable<Type> DependsUpon() {
            return new List<Type>() {typeof (WebserverTask)};
        }

        public void Execute(WebsiteContext context) {
            //Logger.Error("This is bad");
            //Logger.Success("Woho!");
            //Logger.Warn("Not so good");
        }
    }
}