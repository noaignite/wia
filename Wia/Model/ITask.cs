using System;
using System.Collections.Generic;
using Wia.Commands;

namespace Wia.Model {
    internal interface ITask {
        IEnumerable<Type> DependsUpon();
        void Execute(WebsiteContext context);
    }
}