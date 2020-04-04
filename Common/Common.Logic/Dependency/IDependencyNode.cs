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
        HashSet<DependencyNodeKey> DependentNodes { get; }
        HashSet<DependencyNodeKey> DependingNodes { get; }
        DependencyNodeKey DependencyNodeKey { get; }
        bool TraverseDependencyChain(ICheckList checkList, Stack<string> DependencyChain, DependencyNodeKey.Processes Process, Func<IDependencyNode, bool> DependencyChainAction);
        void MarkNodeAsChanged(ICheckList checkList, List<IDependencyNode> roots, DependencyNodeKey.Processes Process);
    }
}

