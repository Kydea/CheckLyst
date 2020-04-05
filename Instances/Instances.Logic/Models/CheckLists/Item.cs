using System;
using System.Collections.Generic;
using System.Linq;
using CheckListManager.Common.Logic.Dependency;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions;
using Newtonsoft.Json;

namespace CheckListManager.Instances.Logic.Models.CheckLists
{
    public abstract class Item : Element, ICheckListItem
    {
        public virtual string CssClass => TypeName;
        public Formula DescriptionFormula { get; } = new Formula();
        public string Description { get; set; }
        public string DescriptionValue { get => DescriptionFormula.Value; }
        public ICheckListGroup Group { get; set; }
        public int Id { get; set; }
        public int ClonedBy { get; set; } //id of the iterator model item from which this iteration item has been cloned
        public int ItemIndex { get; set; } //the intex the item in the parent group
        public ICheckListIteration Iteration { get; set; }
        public bool IterationItem { get; set; }
        public bool IteratorModelItem { get; set; }
        public virtual bool NewLine { get; set; }
        public ICheckListPage Page { get; set; }
        public ICheckListItem Previous { get; set; }
        public abstract string TypeName { get; }
        public int FontSize { get; set; }
        public string Color { get; set; }
        public bool Removed { get; set; }

        public override void Setup()
        {
            if (!IterationItem)
                base.Setup();
            CheckList.AddItem(this);
        }

        public override void BuildDependencies()
        {
            base.BuildDependencies();

            DependencyNodeKey DependencyNodeKey = this.DependencyNodeKey.Clone(DependencyNodeKey.Processes.Other, "description");
            DescriptionFormula.BuildDependencies(Description);
            foreach (var Key in DescriptionFormula.ValueKeys)
                if (CheckList.Expressions.TryGetValue(Key, out var Expression))
                {
                    Expression.DependentNodes.Add(DependencyNodeKey);
                    DependingNodes.Add(Expression.DependencyNodeKey);
                }
        }

        protected override void UpdateValue()
        {
        }

        protected override void UpdateOther()
        {
            Dictionary<string, object> RuntimeValues = new Dictionary<string, object>();
            if (Iteration != null) RuntimeValues["IterationIndex"] = Iteration.Index;
            DescriptionFormula.Evaluate(CheckList, RuntimeValues, Instance.DecimalSeparator, Instance.TimeZone, Iteration?.Indexes);
        }

        protected override void UpdateHidden()
        {
            base.UpdateHidden();
            if (!Hidden && Group != null)
                Hidden = Hidden || Group.Hidden;
        }

        protected override bool IsReadOnlyByDefault() => Previous != null && !Previous.Valid;

        public override void UpdateReadOnly(string userCode, bool readOnlyInstance)
        {
            if (Removed || readOnlyInstance || Group != null && Group.ReadOnly || Page != null && Page.ReadOnly)
                ReadOnly = true;
            else
                base.UpdateReadOnly(userCode, readOnlyInstance);
        }

        public Item FindIteratorModelItem() =>
            IterationItem ? (Item)((Group)((Group)Iteration.Group).FindIteratorModelItem()).Items[ItemIndex] : this;

        public IEnumerable<Item> FindIterationItems()
        {
            if (IteratorModelItem)
            {
                var IterationGroups = ((Group)Group).FindIterationItems();
                var Iterations = IterationGroups.SelectMany(i => ((Group)i).Iterations);
                foreach (var Iteration in Iterations.Where(i => !i.Removed))
                    yield return (Item)Iteration.Items[ItemIndex];
            }
            else
                yield return this;
        }

        public override string GetSignatureInformation() => base.GetSignatureInformation() ?? ((Group)Group)?.GetSignatureInformation() ?? ((Page)Page)?.GetSignatureInformation();

        protected override Dictionary<string, ICheckListElement> GetBranchElements() => Iteration?.Elements ?? base.GetBranchElements();

        public virtual void Remove() =>  Removed = true;

        public abstract ICheckListItem Clone(ICheckListGroup parent);

        protected static void Copy(Item source, Item destination)
        {
            Element.Copy(source, destination);
            destination.Page = source.Page;
            destination.Description = source.Description;
            destination.NewLine = source.NewLine;
            destination.ClonedBy = source.Id;
            destination.IterationItem = true;
            destination.IteratorModelItem = false;
            destination.Color = source.Color;
            destination.FontSize = source.FontSize;

            destination.DescriptionFormula.BuildDependencies(source.Description);
        }

        public override DependencyNodeKey DependencyNodeKey => new DependencyNodeKey("item", Id);
    }
}
