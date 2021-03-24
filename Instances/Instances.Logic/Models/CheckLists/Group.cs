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
        public bool DisableAdd { set { AddIterationButton = !value; AddIterationCondition = value ? Conditions.False : Conditions.True; } } //for compatibility with version < 3.0
        public bool DisableRemove { set { RemoveIterationButton = !value; RemoveIterationCondition = value ? Conditions.False : Conditions.True; } } //for compatibility with version < 3.0
        public bool DisableMove { set { MoveIterationButton = !value; MoveIterationCondition = value ? Conditions.False : Conditions.True; } } //for compatibility with version < 3.0
        public bool AddIterationButton { get; set; }
        public Conditions AddIterationCondition { get; set; } = Conditions.True;
        public string AddIterationExpressionCode { get; set; }
        public bool RemoveIterationButton { get; set; }
        public Conditions RemoveIterationCondition { get; set; } = Conditions.True;
        public string RemoveIterationExpressionCode { get; set; }
        public bool MoveIterationButton { get; set; }
        public Conditions MoveIterationCondition { get; set; } = Conditions.True;
        public string MoveIterationExpressionCode { get; set; }
        public string ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedOn { get; set; }

        public override dynamic GetViewModel()
        {
            var ItemModel = base.GetViewModel();
            if (Iterator)
            {
                ItemModel.Iterator = true;
                ItemModel.Iterations = Iterations.Select(i => ((Iteration)i).GetViewModel());
                ItemModel.AddIterationButton = AddIterationButton;
                ItemModel.RemoveIterationButton = RemoveIterationButton;
                ItemModel.MoveIterationButton = MoveIterationButton;
            }

            ItemModel.Items = Items.Select(i => ((Item)i).GetViewModel());
            if (ShowAsTable) ItemModel.ShowAsTable = true;
            return ItemModel;
        }

        protected override void OnUpdate(DependencyRelation.Properties Property)
        {
            if (Property == DependencyRelation.Properties.AddIteration)
                AddIterationButton =
                    AddIterationCondition == Conditions.True
                    || AddIterationCondition == Conditions.Expression && CheckList?.GetExpressionValue(AddIterationExpressionCode, this)?.Value as bool? == true;
            else if (Property == DependencyRelation.Properties.RemoveIteration)
                RemoveIterationButton =
                    RemoveIterationCondition == Conditions.True
                    || RemoveIterationCondition == Conditions.Expression && CheckList?.GetExpressionValue(RemoveIterationExpressionCode, this)?.Value as bool? == true;
            else if (Property == DependencyRelation.Properties.MoveIteration)
                MoveIterationButton =
                    MoveIterationCondition == Conditions.True
                    || MoveIterationCondition == Conditions.Expression && CheckList?.GetExpressionValue(MoveIterationExpressionCode, this)?.Value as bool? == true;
            else if (Property != DependencyRelation.Properties.Iterations) //at the moment this is handled directly into InstanceManagerService
                base.OnUpdate(Property);
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

        public override bool Setup()
        {
            var Result = base.Setup();
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

            return Result;
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
                        if (item is ICheckListValidityElement checkListValidityElement && !checkListValidityElement.Valid)
                            return false;
            }
            else
            {
                foreach (var item in Items)
                    if (item is ICheckListValidityElement checkListValidityElement && !checkListValidityElement.Valid)
                        return false;
            }
            return true;
        }

        public override void UpdateReadOnly(string userCode, bool readOnlyInstance)
        {
            base.UpdateReadOnly(userCode, readOnlyInstance);

            if (Iterator)
                foreach (var iteration in Iterations)
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
                AddIterationCondition = AddIterationCondition,
                AddIterationExpressionCode = AddIterationExpressionCode,
                RemoveIterationCondition = RemoveIterationCondition,
                RemoveIterationExpressionCode = RemoveIterationExpressionCode,
                MoveIterationCondition = MoveIterationCondition,
                MoveIterationExpressionCode = MoveIterationExpressionCode,
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
                foreach (var iteration in Iterations.Where(i => !i.Removed))
                    iteration.Remove();

            base.Remove();
        }

        protected void SetupItems()
        {
            ICheckListValidityElement previous = null;

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
                if (item is ICheckListValidityElement checkListValidityElement)
                    previous = checkListValidityElement;
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
            if (Iterator)
            {
                MarkNodeAsChanged(CheckList, Roots, DependencyRelation.Properties.AddIteration);
                MarkNodeAsChanged(CheckList, Roots, DependencyRelation.Properties.RemoveIteration);
                MarkNodeAsChanged(CheckList, Roots, DependencyRelation.Properties.MoveIteration);
            }

            var Iterations = this.Iterations.Where(i => !i.Removed);
            bool AnyIteration = Iterations.Any();

            foreach (Iteration iteration in Iterations)
                iteration.MarkAllAsChanged(Roots);

            foreach (Item item in Items)
                if (!item.IteratorModelItem || !AnyIteration) //when there is no iteration the table header must be marked explicitely, could be imporved by checking if an ancestor is a table
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
                    if (HiddenCondition != Conditions.True)
                        Item.AddDependingRelation(DependencyRelation.Properties.Hidden, this, DependencyRelation.Properties.Hidden);

                    if (Item is ICheckListEditableItem && (ValidCondition == Conditions.Default || ValidCondition == Conditions.ExpressionOrDefault || ValidCondition == Conditions.ExpressionAndDefault))
                        AddDependingRelation(DependencyRelation.Properties.Validity, Item, DependencyRelation.Properties.Validity);
                }

                if (!(IterationItem && Item.IteratorModelItem)) //the cloned iterator model items are ignored
                    Item.BuildDependencies();
            }

            if (Iterator)
            {
                if (AddIterationCondition == Conditions.Expression)
                    GetExpression(AddIterationExpressionCode, DependencyRelation.Properties.AddIteration, "add-iteration");
                if (RemoveIterationCondition == Conditions.Expression)
                    GetExpression(RemoveIterationExpressionCode, DependencyRelation.Properties.RemoveIteration, "remove-iteration");
                if (MoveIterationCondition == Conditions.Expression)
                    GetExpression(MoveIterationExpressionCode, DependencyRelation.Properties.MoveIteration, "move-iteration");
            }

            if (Iterator) //this is called only for instances existing before v. 2.0.0, because BuildDependencies is called only once per instance when no iteration exists
                foreach (Iteration Iteration in Iterations.Where(i => !i.Removed))
                    Iteration.BuildDependencies();
        }

        public override string ToString() => $"group {Code ?? Id.ToString()}";
    }
}
