using CheckListManager.Common.Logic.Models.CheckLists;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.Dependency
{
    public interface IDependencyNode
    {
        List<DependencyNodeKey> DependentNodes { get; }
        List<DependencyNodeKey> DependingNodes { get; }
        DependencyNodeKey DependencyNodeKey { get; }
        bool TraverseDependencyChain(ICheckList checkList, Stack<IDependencyNode> DependencyChain, string DependencyChainKey, Func<IDependencyNode, bool> DependencyChainAction);
    }
}

