using System;
using System.Collections.Generic;
using System.Linq;
using CheckListManager.Common.Logic.Dependency;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions;
using Newtonsoft.Json;

namespace CheckListManager.Instances.Logic.Models.CheckLists
{
    public class Iteration : Group, ICheckListIteration
    {
        public override string TypeName => "Iteration";

        [JsonIgnore]
        public int Index { get; set; }

        [JsonIgnore]
        public List<int> Indexes { get; set; } = new List<int>();

        [JsonIgnore]
        public TypedValue ReccurencyValue { get; set; }

        private void AddAvailableSubItem(ICheckListGroup Group)
        {
            if (!Group.Iterator)
                Group.Items.ForEach(Item =>
                {
                    if (!string.IsNullOrEmpty(Item.Code)) ElementsList.Add(Item);
                    if (Item is ICheckListGroup group) AddAvailableSubItem(group);
                });
        }

        private void GetAllChildren(ICheckListGroup Group, List<DependencyNode> children)
        {
            if (!Group.Iterator)
                Group.Items.ForEach(Item =>
                {
                    children.Add((Element)Item);
                    if (Item is ICheckListGroup group) GetAllChildren(group, children);
                });
        }

        public void GetAllChildren(List<DependencyNode> children) => GetAllChildren(this, children);

        public override bool Setup()
        {
            //loads or setup for the first time the iteration
            var Loading = Id > 0;
            CheckList.AddItem(this);
            IterationItem = true;
            if (Loading)
                foreach (var Item in Items)
                {
                    Item.Iteration = this;
                    Item.IterationItem = true;
                }
            else
                foreach (var Item in Group.Items)
                {
                    var Clone = Item.Clone(this);
                    Clone.Iteration = this;
                    Items.Add(Clone);
                }
            SetupItems();

            ElementsList = new List<ICheckListElement>();
            AddAvailableSubItem(this);

            return true;
        }

        [JsonIgnore]
        public List<ICheckListElement> ElementsList { get; private set; }

        public Dictionary<string, ICheckListElement> Elements
        {
            get
            {
                var Result = new Dictionary<string, ICheckListElement>(ElementsList.Count);
                ElementsList.ForEach(x => Result.Add(x.Code, x));
                var Parent = Group;
                while (Parent != null)
                    if (!Parent.IterationItem)
                        Parent = null;
                    else
                    {
                        if (Parent is Iteration Iteration)
                            Iteration.ElementsList.ForEach(x => Result.Add(x.Code, x));
                        Parent = Parent.Group;
                    }

                ((Instance)CheckList).ElementsList.ForEach(x => Result.Add(x.Code, x));
                return Result;
            }
        }

        public override void BuildDependencies()
        {
            //this is called only for instances existing before v. 2.0.0, because BuildDependencies is called only once per instance
            for (var Index = 0; Index < Items.Count(); ++Index)
            {
                var Item = (Item)Items[Index];
                Item.ClonedBy = Group.Items[Index].Id;
            }
            base.BuildDependencies();
        }

        public override void MarkAllAsChanged(List<Tuple<DependencyNode, DependencyRelation.Properties>> Roots)
        {
            foreach (Item item in Items)
                if (!item.IteratorModelItem)
                    item.MarkAllAsChanged(Roots);
        }

        public override bool Valid => true;
    }
}
