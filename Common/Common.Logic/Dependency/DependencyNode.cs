using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions;
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

        [JsonIgnore]
        public abstract string DependencyClassName { get; }
        [JsonIgnore]
        public virtual string DependencyObjectCode { get; }
        [JsonIgnore]
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
        protected HashSet<DependencyRelation.Properties> Checked { get; } = new HashSet<DependencyRelation.Properties>();

        public abstract bool Update(DependencyRelation.Properties Property);

        public virtual void MarkNodeAsChanged(ICheckList CheckList, List<Tuple<DependencyNode, DependencyRelation.Properties>> Roots, DependencyRelation.Properties Property)
        {
            if (Property == DependencyRelation.Properties.ReadOnly) return; //readonly is not stateless and is always updated for each node
            if (NeedToUpdate.Contains(Property)) return; //Node already marked for this process

            NeedToUpdate.Add(Property);

            var DependentRelations = this.DependentRelations.Where(x => x.SourceProperty == Property && x.DestinationProperty != DependencyRelation.Properties.ReadOnly); //readonly is not stateless and is always updated for each node
            if (DependentRelations.Any())
                foreach (var DependentRelation in DependentRelations)
                    DependentRelation.GetNode(CheckList).MarkNodeAsChanged(CheckList, Roots, DependentRelation.DestinationProperty);
            else
                Roots.Add(new Tuple<DependencyNode, DependencyRelation.Properties>(this, Property));
        }

        private HashSet<DependencyRelation.Properties> DependencyChainTouchedProcesses = new HashSet<DependencyRelation.Properties>();
        public bool TraverseDependencyChain(ICheckList CheckList, DependencyRelation.Properties Property)
            => TraverseDependencyChain(CheckList, Property, null, false);
        public bool TestDependencyChain(ICheckList CheckList, DependencyRelation.Properties Property)
            => TraverseDependencyChain(CheckList, Property, new Stack<Tuple<DependencyNode, DependencyRelation.Properties>>(), true);
        private bool TraverseDependencyChain(ICheckList CheckList, DependencyRelation.Properties Property, Stack<Tuple<DependencyNode, DependencyRelation.Properties>> DependencyChain, bool TestCycles)
        {
            if (!TestCycles)
            {
                //process only the updated properties
                if (!NeedToUpdate.Contains(Property)) return true;
                NeedToUpdate.Remove(Property);
            }

            var Result = true;
            try
            {
                if (TestCycles)
                {
                    DependencyChain.Push(new Tuple<DependencyNode, DependencyRelation.Properties>(this, Property));
                    if (DependencyChainTouchedProcesses.Contains(Property))
                    {
                        var ErrorMessage = $"Dependency cycle found: {string.Join("> ", DependencyChain.Reverse().Select(x => $"{DependencyRelation.GetPropertyDescription(x.Item2)} of {x.Item1.DependencyClassName} {x.Item1.DependencyObjectCode ?? x.Item1.DependencyObjectID?.ToString()} "))}";
                        DependencyChain.ToList().ForEach(x =>
                        {
                            if (x.Item1 is Expression Expression) Expression.Error = ErrorMessage;
                        });
                        return false;
                        //throw new Exception($"Dependency cycle found: {string.Join("> ", DependencyChain.Reverse().Select(x => $"{DependencyRelation.GetPropertyDescription(x.Item2)} of {x.Item1.DependencyClassName} {x.Item1.DependencyObjectCode ?? x.Item1.DependencyObjectID?.ToString()} "))}");
                    }
                    DependencyChainTouchedProcesses.Add(Property);

                    //checks the cycle just once per node
                    if (Checked.Contains(Property)) return true;
                    Checked.Add(Property);
                }

                foreach (var DependingRelation in DependingRelations.Where(n => n.SourceProperty == Property))
                    if (!DependingRelation.GetNode(CheckList).TraverseDependencyChain(CheckList, DependingRelation.DestinationProperty, DependencyChain, TestCycles))
                        Result = false;

                if (Result)
                    Update(Property);
            }
            finally
            {
                if (TestCycles)
                {
                    DependencyChain.Pop();
                    DependencyChainTouchedProcesses.Remove(Property);
                }
            }
            return Result;
        }

        public void UpdateChain(ICheckList CheckList, DependencyRelation.Properties Property)
        {
            UpdateNodesChains(CheckList, Property, new DependencyNode[] { this });
        }

        public static void UpdateNodes(ICheckList CheckList, IEnumerable<Tuple<DependencyNode, DependencyRelation.Properties>> Roots)
        {
            foreach (var Relation in Roots)
                Relation.Item1.TraverseDependencyChain(CheckList, Relation.Item2);
        }

        public static void UpdateNodesChains(ICheckList CheckList, DependencyRelation.Properties Property, IEnumerable<DependencyNode> Nodes)
        {
            var Roots = new List<Tuple<DependencyNode, DependencyRelation.Properties>>();
            foreach (var Node in Nodes)
                Node.MarkNodeAsChanged(CheckList, Roots, Property);
            UpdateNodes(CheckList, Roots);
        }
    }
}

