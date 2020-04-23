using System;
using System.Collections.Generic;
using CheckListManager.Common.Logic.Dependency;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions;

namespace CheckListManager.Prototypes.Logic.Models.CheckLists
{
	public abstract class Item : Element, ICheckListItem
	{
		public virtual string CssClass => TypeName;
        private Formula DescriptionFormula { get; } = new Formula();
		public string Description { get; set; }
		public ICheckListGroup Group { get; set; }
		public int Id { get; set; }
		public ICheckListIteration Iteration { get; set; }
		public bool IterationItem { get; set; }
		public bool IteratorModelItem { get; set; }
		public virtual bool NewLine { get; set; }
		public ICheckListPage Page { get; set; }
        public virtual int? PageIndex { get; set; }
		public abstract string TypeName { get; }
        public int FontSize { get; set; }
        public string Color { get; set; }
        public bool Removed { get; set; }

        public abstract ICheckListItem Clone(ICheckListGroup parent);

        public override void Setup()
        {
            base.Setup();
            CheckList.AddItem(this);
        }

        protected static void Copy(Item source, Item destination)
        {
            Element.Copy(source, destination);
            destination.Page = source.Page;
            destination.Description = source.Description;
            destination.NewLine = source.NewLine;
            destination.IterationItem = true;
            destination.IteratorModelItem = false;
            destination.Color = source.Color;
            destination.FontSize = source.FontSize;
        }

        public void Remove()
		{
			throw new NotImplementedException();
		}

        public override void BuildDependencies()
        {
            base.BuildDependencies();

            DescriptionFormula.BuildDependencies(Description, null);
            foreach (var Key in DescriptionFormula.ValueKeys)
                if (CheckList.Expressions.TryGetValue(Key, out var Expression))
                    AddDependingRelation(DependencyRelation.Properties.Formula, Expression, DependencyRelation.Properties.Value);
        }

        public override string DependencyClassName => "item";
        public override int? DependencyObjectID => Id;
        public override string ToString() => $"item {Code ?? Id.ToString()}";
    }
}
