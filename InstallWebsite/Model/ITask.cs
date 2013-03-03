using System;
using System.Collections.Generic;

namespace InstallWebsite.Model {
    internal interface ITask {
        IEnumerable<Type> DependsUpon();
        void Execute(WebsiteContext context);
    }
}