using System.Collections.Generic;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Instances.Logic.Models.CheckLists
{
	public class Group : Item, ICheckListGroup
	{
		public override string TypeName => "Group";

		public virtual Dictionary<string, ICheckListElement> Elements { get; set; }

		public List<ICheckListItem> Items { get; set; }
		public List<ICheckListIteration> Iterations { get; } = new List<ICheckListIteration>();
		public bool Iterator { get; set; }
	}
}
