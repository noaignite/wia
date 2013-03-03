using System;
using System.Collections.Generic;

namespace InstallWebsite {
    internal interface ITask {
        IEnumerable<Type> DependsUpon();
        void Execute(WebsiteContext context);
    }
}