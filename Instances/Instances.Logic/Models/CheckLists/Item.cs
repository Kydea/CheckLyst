using System.Collections.Generic;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions;

namespace CheckListManager.Instances.Logic.Models.CheckLists
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

		public override void Setup()
		{
			base.Setup();
			CheckList.AddItem(this);
		}

		public override void Update()
		{
			var parent = Group;
			while (parent != null)
				if (parent is Iteration iteration)
				{
					RuntimeValues["IterationIndex"] = iteration.Index;
					parent = null;
				}
				else
					parent = parent.Group;

			DescriptionFormula?.Update(CheckList.Expressions, RuntimeValues);
		}

		public override void UpdateReadOnly(string userCode, bool canEdit)
		{
			base.UpdateReadOnly(userCode, canEdit);

			if (canEdit)
			{
				IsReadOnly = Group != null && Group.IsReadOnly ||
							 Page != null && Page.IsReadOnly;
			}

		}

		public virtual void Remove()
		{
			((Instance)CheckList).Items.Remove(Id);
		}

		public abstract ICheckListItem Clone(ICheckListGroup parent);

		protected static void Copy(Item source, Item destination)
		{
			Element.Copy(source, destination);
			destination.Page = source.Page;
			destination.Description = source.Description;
			destination.NewLine = source.NewLine;
			destination.DescriptionFormula = source.DescriptionFormula == null ? null : new Formula(source.DescriptionFormula.RawValue);
			destination.IterationItem = true;
			destination.IteratorModelItem = false;
		}
	}
}
