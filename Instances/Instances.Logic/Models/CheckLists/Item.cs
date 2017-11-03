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
	}
}
