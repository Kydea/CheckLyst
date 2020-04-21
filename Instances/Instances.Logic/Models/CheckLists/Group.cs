using System.Collections.Generic;
using System.Linq;
using CheckListManager.Common.Logic.Models.CheckLists;
using System;
using CheckListManager.Instances.Logic.Exceptions;
using System.Threading.Tasks;
using CheckListManager.Common.Logic.Dependency;

namespace CheckListManager.Instances.Logic.Models.CheckLists
{
    public class Group : Item, ICheckListGroup
    {
        public override string TypeName => "Group";

        public List<ICheckListItem> Items { get; set; } = new List<ICheckListItem>();
        public List<ICheckListIteration> Iterations { get; } = new List<ICheckListIteration>();
        public bool Iterator { get; set; }

        public bool ShowAsTable { get; set; }

        public int InstanceIterations { get; set; }

        public int PrototypeIterations { get; set; }

        public bool DisableAdd { get; set; }

        public bool DisableRemove { get; set; }

        public bool DisableMove { get; set; }

        public override dynamic GetViewModel()
        {
            var ItemModel = base.GetViewModel();
            if (Iterator)
            {
                ItemModel.Iterator = true;
                ItemModel.Iterations = Iterations.Select(i => ((Iteration)i).GetViewModel());
                if (DisableAdd) ItemModel.DisableAdd = true;
                if (DisableRemove) ItemModel.DisableRemove = true;
                if (DisableMove) ItemModel.DisableMove = true;
            }

            ItemModel.Items = Items.Select(i => ((Item)i).GetViewModel());
            if (ShowAsTable) ItemModel.ShowAsTable = true;
            return ItemModel;
        }

        protected override void UpdateHidden()
        {
            base.UpdateHidden();
            if (!Hidden && Page != null)
                Hidden = Page.Hidden;
        }

        public void CreateInstanceIterations()
        {
            if (Iterator && InstanceIterations > 0 && !IteratorModelItem)
            {
                for (int Index = 0; Index < InstanceIterations; ++Index)
                    AddIteration();

                foreach (var iteration in Iterations)
                    iteration.CreateInstanceIterations();
            }

            if (!Iterator)
                foreach (var Item in Items)
                    if (Item is Group Group)
                        Group.CreateInstanceIterations();
        }

        public override void Setup()
        {
            base.Setup();
            SetupItems();
            if (Iterator)
            {
                SetupIterationsIndexes();
                foreach (var Iteration in Iterations.Where(i => !i.Removed))
                {
                    Iteration.CheckList = CheckList;
                    Iteration.Page = Page;
                    Iteration.Group = this;
                    Iteration.Setup();
                }
            }
        }

        public void SetupIterationsIndexes()
        {
            var Index = 0;
            foreach (Item Item in Items.Where(i => !i.IteratorModelItem))
                if (Item is Group Group)
                    Group.SetupIterationsIndexes();

            foreach (Iteration Iteration in Iterations)
            {
                Iteration.Indexes.Clear();
                if (this.Iteration != null)
                    this.Iteration.Indexes.ForEach(i => Iteration.Indexes.Add(i));

                Iteration.Indexes.Add(Index); //0-base
                Iteration.Index = Index + 1; //1-base
                if (!Iteration.Removed) ++Index;

                Iteration.SetupIterationsIndexes();
            }

        }

        public override bool IsValidByDefault()
        {
            if (Iterator)
            {
                foreach (var Iteration in Iterations.Where(i => !i.Removed))
                    foreach (var item in Iteration.Items) //the iteration are never updated so they are considered transparent to validity
                        if (!item.Valid)
                            return false;
            }
            else
            {
                foreach (var item in Items)
                    if (!item.Valid)
                        return false;
            }
            return true;
        }

        public override void UpdateReadOnly(string userCode, bool readOnlyInstance)
        {
            base.UpdateReadOnly(userCode, readOnlyInstance);

            if (Iterator)
                foreach (var iteration in Iterations.Where(i => !i.Removed))
                    ((Iteration)iteration).UpdateReadOnly(userCode, readOnlyInstance);
            else
                foreach (var item in Items)
                    ((Item)item).UpdateReadOnly(userCode, readOnlyInstance);

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

        private void CheckSignatures()
        {
            foreach (var Item in Items)
                if (Item is Group Group)
                    Group.CheckSignatures();
                else if (Item is Signature && ((Signature)Item).Signed)
                    throw new InstanceException("The iteration contains issued signatures and cannot be removed");

            if (Iterator)
                foreach (var Iteration in Iterations.Where(i => !i.Removed))
                    ((Iteration)Iteration).CheckSignatures();
        }

        public override void Remove()
        {
            CheckSignatures();

            foreach (var item in Items)
                item.Remove();

            if (Iterator)
                foreach (var iteration in Iterations)
                    iteration.Remove();

            base.Remove();
        }

        protected void SetupItems()
        {
            ICheckListItem previous = null;

            int ItemIndex = 0;
            foreach (Item item in Items)
            {
                item.CheckList = CheckList;
                item.Page = Page;
                item.Group = this;
                item.Previous = previous;
                item.IteratorModelItem = Iterator || IteratorModelItem;
                item.Iteration = item.Iteration ?? Iteration;
                item.ItemIndex = ItemIndex++;
                item.Setup();
                previous = item;
            }
        }

        public Iteration AddIteration()
        {
            var iteration = new Iteration
            {
                CheckList = CheckList,
                Page = Page,
                Group = this
            };
            iteration.Setup();
            Iterations.Add(iteration);
            return iteration;
        }

        public void Move(Iteration iteration, int direction)
        {
            int
                from = Iterations.IndexOf(iteration),
                to = from + direction;
            while (Iterations[to].Removed)
                to += direction;
            Iterations.RemoveAt(from);
            Iterations.Insert(to, iteration);
        }

        public void Remove(Iteration iteration)
        {
            iteration.Remove();
        }

        public override void MarkAllAsChanged(List<Tuple<DependencyNode, DependencyRelation.Properties>> Roots)
        {
            base.MarkAllAsChanged(Roots);

            foreach (Iteration iteration in Iterations)
                iteration.MarkAllAsChanged(Roots);

            foreach (Item item in Items)
                item.MarkAllAsChanged(Roots);
        }


        public override void ClearDependencies()
        {
            base.ClearDependencies();
            foreach (Item Item in Items)
                Item.ClearDependencies();
        }

        public override void BuildDependencies()
        {
            base.BuildDependencies();
            foreach (Item Item in Items)
            {
                if (!IterationItem)
                {
                    if (HiddenCondition == Conditions.False || HiddenCondition == Conditions.Expression)
                        Item.AddDependingRelation(DependencyRelation.Properties.Hidden, this, DependencyRelation.Properties.Hidden);

                    if (ValidCondition == Conditions.Default || ValidCondition == Conditions.ExpressionOrDefault || ValidCondition == Conditions.ExpressionAndDefault)
                        AddDependingRelation(DependencyRelation.Properties.Validity, Item, DependencyRelation.Properties.Validity);
                }
                Item.BuildDependencies();
            }

            if (Iterator) //this is called only for instances existing before v. 2.0.0, because BuildDependencies is called only once per instance when no iteration exists
                foreach (Iteration Iteration in Iterations.Where(i => !i.Removed))
                    Iteration.BuildDependencies();
        }

        public override string ToString() => $"group {Code ?? Id.ToString()}";
    }
}
