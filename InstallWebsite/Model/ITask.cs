using System;
using System.Collections.Generic;

namespace Wia.Model {
    internal interface ITask {
        IEnumerable<Type> DependsUpon();
        void Execute(WebsiteContext context);
    }
}