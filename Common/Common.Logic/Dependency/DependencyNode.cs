using CheckListManager.Common.Logic.Models.CheckLists;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.Dependency
{
    public abstract class DependencyNode
    {
        [JsonProperty]
        private HashSet<DependencyRelation> DependentRelations { get; } = new HashSet<DependencyRelation>();
        [JsonProperty]
        private HashSet<DependencyRelation> DependingRelations { get; } = new HashSet<DependencyRelation>();

        public abstract string DependencyClassName { get; }
        public virtual string DependencyObjectCode { get; }
        public virtual int? DependencyObjectID { get; }

        public virtual void ClearDependencies()
        {
            DependingRelations.Clear();
            DependentRelations.Clear();
        }

        public void AddDependingRelation(DependencyRelation.Properties SourceProperty, DependencyNode DependingNode, DependencyRelation.Properties DestinationProperty)
        {
            DependingRelations.Add(new DependencyRelation(SourceProperty, DependingNode, DestinationProperty));
            DependingNode.DependentRelations.Add(new DependencyRelation(DestinationProperty, this, SourceProperty));
        }

        protected HashSet<DependencyRelation.Properties> NeedToUpdate { get; } = new HashSet<DependencyRelation.Properties>();

        public abstract bool Update(DependencyRelation.Properties Property);

        public void MarkNodeAsChanged(ICheckList CheckList, List<Tuple<DependencyNode, DependencyRelation.Properties>> Roots, DependencyRelation.Properties Property)
        {
            if (Property == DependencyRelation.Properties.ReadOnly) return; //readonly is not stateless and is always updated for each node
            if (NeedToUpdate.Contains(Property)) return; //Node already marked for this process

            NeedToUpdate.Add(Property);

            var DependentRelations = this.DependentRelations.Where(n => n.SourceProperty == Property);
            if (DependentRelations.Any())
                foreach (var DependentRelation in DependentRelations)
                    DependentRelation.GetNode(CheckList).MarkNodeAsChanged(CheckList, Roots, DependentRelation.DestinationProperty);
            else
                Roots.Add(new Tuple<DependencyNode, DependencyRelation.Properties>(this, Property));

        }

        private HashSet<DependencyRelation.Properties> DependencyChainTouchedProcesses = new HashSet<DependencyRelation.Properties>();
        public bool TraverseDependencyChain(ICheckList CheckList, DependencyRelation.Properties Property, Stack<Tuple<DependencyNode, DependencyRelation.Properties>> DependencyChain)
        {
            if (!NeedToUpdate.Contains(Property)) return true;

            var Result = true;
            try
            {
                DependencyChain.Push(new Tuple<DependencyNode, DependencyRelation.Properties>(this, Property));
                if (DependencyChainTouchedProcesses.Contains(Property))
                    throw new Exception($"Dependency cycle found: {string.Join("> ", DependencyChain.Reverse().Select(x => $"{DependencyRelation.GetPropertyDescription(x.Item2)} of {x.Item1.DependencyClassName} {x.Item1.DependencyObjectCode ?? x.Item1.DependencyObjectID?.ToString()} "))}");
                DependencyChainTouchedProcesses.Add(Property);

                foreach (var DependingRelation in DependingRelations.Where(n => n.SourceProperty == Property))
                    if (!DependingRelation.GetNode(CheckList).TraverseDependencyChain(CheckList, DependingRelation.DestinationProperty, DependencyChain))
                        Result = false;

                if (Result)
                    Update(Property);
            }
            finally
            {
                NeedToUpdate.Remove(DependencyRelation.Properties.Value);
                DependencyChain.Pop();
                DependencyChainTouchedProcesses.Remove(Property);
            }
            return Result;
        }

        public void UpdateChain(ICheckList CheckList, DependencyRelation.Properties Property)
        {
            UpdateNodesChains(CheckList, Property, new DependencyNode[] { this });
        }

        private static readonly Stack<Tuple<DependencyNode, DependencyRelation.Properties>> DependencyChain = new Stack<Tuple<DependencyNode, DependencyRelation.Properties>>();

        public static void UpdateNodes(ICheckList CheckList, IEnumerable<Tuple<DependencyNode, DependencyRelation.Properties>> Roots)
        {
            foreach (var Relation in Roots)
            {
                DependencyChain.Clear();
                Relation.Item1.TraverseDependencyChain(CheckList, Relation.Item2, DependencyChain);
            }
        }

        public static void UpdateNodesChains(ICheckList CheckList, DependencyRelation.Properties Property, IEnumerable<DependencyNode> Nodes)
        {
            var Roots = new List<Tuple<DependencyNode, DependencyRelation.Properties>>();
            foreach (var Node in Nodes)
            {
                DependencyChain.Clear();
                Node.MarkNodeAsChanged(CheckList, Roots, Property);
            }
            UpdateNodes(CheckList, Roots);
        }
    }
}

