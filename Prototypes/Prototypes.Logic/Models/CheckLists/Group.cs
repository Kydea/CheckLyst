using System.Collections.Generic;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Prototypes.Logic.Models.CheckLists
{
	public class Group : Item, ICheckListGroup
	{
		public override string TypeName => "Group";

		public virtual Dictionary<string, ICheckListElement> Elements { get; set; }

        public List<ICheckListItem> Items { get; set; } = new List<ICheckListItem>();
        public List<ICheckListIteration> Iterations { get; }
		public bool Iterator { get; set; }

        public override void Setup()
        {
            base.Setup();
            foreach (var Item in Items)
            {
                Item.CheckList = CheckList;
                Item.Setup();
            }
        }
    }
}
