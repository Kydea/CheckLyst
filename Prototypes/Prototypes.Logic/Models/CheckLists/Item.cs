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
		public string Description { get; set; }
		public Formula DescriptionFormula { get; set; }
		public ICheckListGroup Group { get; set; }
		public int Id { get; set; }
		public ICheckListIteration Iteration { get; set; }
		public bool IterationItem { get; set; }
		public bool IteratorModelItem { get; set; }
		public virtual bool NewLine { get; set; }
		public ICheckListPage Page { get; set; }
		public ICheckListItem Previous { get; set; }
		public Dictionary<string, object> RuntimeValues { get; } = new Dictionary<string, object>();
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
            destination.DescriptionFormula = source.DescriptionFormula == null ? null : new Formula(source.DescriptionFormula.RawValue);
            destination.IterationItem = true;
            destination.IteratorModelItem = false;
            destination.Color = source.Color;
            destination.FontSize = source.FontSize;
        }

        public void Remove()
		{
			throw new NotImplementedException();
		}

        public override DependencyNodeKey DependencyNodeKey => base.DependencyNodeKey ?? new DependencyNodeKey("item", Id);
    }
}
