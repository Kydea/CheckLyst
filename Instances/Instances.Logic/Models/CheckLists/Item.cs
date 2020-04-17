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
        public int IteratorModelItemId { get; set; }
        public virtual bool NewLine { get; set; }
        public ICheckListPage Page { get; set; }
        public ICheckListItem Previous { get; set; }
        public abstract string TypeName { get; }
        public int FontSize { get; set; }
        public string Color { get; set; }
        public bool Removed { get; set; }

        public override void Setup()
        {
            if (!IterationItem) base.Setup();
            CheckList.AddItem(this);
        }

        protected Dictionary<string, object> GetRuntimeValues()
        {
            Dictionary<string, object> RuntimeValues = new Dictionary<string, object>();
            if (Iteration != null) RuntimeValues["IterationIndex"] = Iteration.Index;
            return RuntimeValues;
        }

        public override void BuildDependencies()
        {
            if (!(this is Iteration))
                DescriptionFormula.BuildDependencies(Description, Iteration == null ? null : FindIteratorModelItem().DescriptionFormula);

            if (!IterationItem)
            {
                foreach (var Key in DescriptionFormula.ValueKeys)
                    if (CheckList.Expressions.TryGetValue(Key, out var Expression))
                        AddDependingRelation(DependencyRelation.Properties.Formula, Expression, DependencyRelation.Properties.Value);

                base.BuildDependencies();
            }
        }

        protected virtual void OnUpdate(DependencyRelation.Properties Property)
        {
            if (Property == DependencyRelation.Properties.Formula)
                UpdateFormula();
            else
                base.Update(Property);
        }

        public sealed override bool Update(DependencyRelation.Properties Property)
        {
            if (IteratorModelItem)
            {
                var IterationItems = FindIterationItems();
                foreach (var IterationItem in IterationItems)
                {
                    IterationItem.NeedToUpdate.Add(Property);
                    IterationItem.OnUpdate(Property);
                }
                NeedToUpdate.Remove(Property);
            }
            else
                OnUpdate(Property);

            return true;
        }

        protected virtual void UpdateFormula()
        {
            if (IterationItem)
                DescriptionFormula.Evaluate(CheckList, GetRuntimeValues(), Instance.DecimalSeparator, Instance.TimeZone, Iteration?.Indexes, FindIteratorModelItem().DescriptionFormula);
            else
                DescriptionFormula.Evaluate(CheckList, GetRuntimeValues(), Instance.DecimalSeparator, Instance.TimeZone, Iteration?.Indexes);

        }

        protected override void UpdateHidden()
        {
            base.UpdateHidden();
            if (!Hidden && Group != null)
                Hidden = Group.Hidden;
        }

        protected override bool IsReadOnlyByDefault() => Previous != null && !Previous.Valid;

        public override void UpdateReadOnly(string userCode, bool readOnlyInstance)
        {
            if (Removed || readOnlyInstance || Group != null && Group.ReadOnly || Page != null && Page.ReadOnly)
                ReadOnly = true;
            else
                base.UpdateReadOnly(userCode, readOnlyInstance);
        }

        public Item FindIteratorModelItem()
        {
            Item IteratorModelItem;
            if (IteratorModelItemId > 0)
                IteratorModelItem = (Item)Instance.Items[IteratorModelItemId];
            else
            {
                IteratorModelItem = IterationItem ? (Item)((Group)((Group)Iteration.Group).FindIteratorModelItem()).Items[ItemIndex] : this;
                IteratorModelItemId = IteratorModelItem.Id;
            }
            return IteratorModelItem;
        }

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

        public override void MarkNodeAsChanged(ICheckList CheckList, List<Tuple<DependencyNode, DependencyRelation.Properties>> Roots, DependencyRelation.Properties Property)
        {
            if (IterationItem)
            {
                var IteratorModel = FindIteratorModelItem();
                IteratorModel.MarkNodeAsChanged(CheckList, Roots, Property);
            }
            else 
                base.MarkNodeAsChanged(CheckList, Roots, Property);
        }

        public override void MarkAllAsChanged(List<Tuple<DependencyNode, DependencyRelation.Properties>> Roots)
        {
            if (IterationItem)
            {
                var IteratorModel = FindIteratorModelItem();
                IteratorModel.MarkAllAsChanged(Roots);
            }
            else
            {
                base.MarkAllAsChanged(Roots);
                MarkNodeAsChanged(CheckList, Roots, DependencyRelation.Properties.Formula);
            }
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

            destination.DescriptionFormula.IsSet = source.FindIteratorModelItem().DescriptionFormula.IsSet;
        }

        public override dynamic GetViewModel()
        {
            var elementModel = base.GetViewModel();
            elementModel.DescriptionValue = DescriptionValue;
            elementModel.Id = Id;
            elementModel.TypeName = TypeName;
            elementModel.NewLine = NewLine;
            elementModel.Color = Color;
            elementModel.FontSize = FontSize;

            return elementModel;
        }

        public override string DependencyClassName => "item";
        public override int? DependencyObjectID => Id;
        public override string DependencyObjectCode => IterationItem ? null : Code;
    }
}
