using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CheckListManager.Common.Logic.Dependency;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Prototypes.Logic.Models.CheckLists
{
    public class Group : Item, ICheckListGroup
    {
        public override string TypeName => "Group";

        public virtual Dictionary<string, ICheckListElement> Elements { get; set; }

        public List<ICheckListItem> Items { get; set; } = new List<ICheckListItem>();
        public List<ICheckListIteration> Iterations { get; } = new List<ICheckListIteration>();
        public bool Iterator { get; set; }

        public bool ShowAsTable { get; set; }

        public int InstanceIterations { get; set; }

        public int PrototypeIterations { get; set; }

        public bool DisableAdd { get; set; }

        public bool DisableRemove { get; set; }

        public bool DisableMove { get; set; }

        private bool InTable { get; set; }

        public override void Setup()
        {
            base.Setup();

            foreach (var Item in Items)
            {
                Item.CheckList = CheckList;
                Item.Group = this;
                if ((ShowAsTable || InTable) && Item is Group group) group.InTable = true;
                if (InTable) //disable hidden properties for subgroups of tables
                {
                    ShowAsTable = false;
                    Iterator = false;
                }
                Item.Setup();
            }
        }

        public void Traverse(Func<ICheckListItem, bool> condition, Action<ICheckListItem> action)
        {
            if (condition(this))
                action(this);

            foreach (var iteration in Iterations)
                iteration.Traverse(condition, action);

            foreach (var item in Items)
                if (item is Group group)
                    group.Traverse(condition, action);
                else if (condition(item))
                    action(item);
        }

        public async Task TraverseAsync(Func<ICheckListItem, bool> condition, Func<ICheckListItem, Task> action)
        {
            if (condition(this))
                await action(this);

            foreach (var iteration in Iterations)
                await iteration.TraverseAsync(condition, action);

            foreach (var item in Items)
                if (item is Group group)
                    await group.TraverseAsync(condition, action);
                else if (condition(item))
                    await action(item);
        }

        public void AddIterations(int Count)
        {
            for (int Index = 0; Index < Count; ++Index)
            {
                var iteration = new Iteration
                {
                    CheckList = CheckList,
                    Page = Page,
                    Group = this
                };
                iteration.Setup();
                Iterations.Add(iteration);
            }
        }

        public override ICheckListItem Clone(ICheckListGroup parent)
        {
            var group = new Group
            {
                CheckList = CheckList,
                Iterator = Iterator,
                ShowAsTable = ShowAsTable,
                InstanceIterations = InstanceIterations,
                PrototypeIterations = PrototypeIterations,
                DisableAdd = DisableAdd,
                DisableRemove = DisableRemove,
                DisableMove = DisableMove,
                Group = parent
            };
            foreach (var item in Items)
                group.Items.Add(item.Clone(group));

            Copy(this, group);
            return group;
        }

        public override void BuildDependencies()
        {
            base.BuildDependencies();
            foreach (Item Item in Items)
            {
                Item.CheckList = CheckList;
                Item.BuildDependencies();
            }
        }

        public override void Check()
        {
            base.Check();
            foreach (Item Item in Items)
            {
                Item.CheckList = CheckList;
                Item.Check();
            }
        }

        public void CreateInstanceIterations() { throw new NotImplementedException(); }

        protected override IEnumerable<DependencyNodeKey> GetDependingNodes()
        {
            foreach (var DependencyNodeKey in base.GetDependingNodes())
                yield return DependencyNodeKey;
            foreach (Item Item in Items)
                yield return Item.DependencyNodeKey;
        }

    }
}
