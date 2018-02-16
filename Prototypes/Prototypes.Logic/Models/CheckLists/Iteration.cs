using CheckListManager.Common.Logic.Models.CheckLists;
using System.Collections.Generic;
using System.Linq;

namespace CheckListManager.Prototypes.Logic.Models.CheckLists
{
	public class Iteration : Group, ICheckListIteration
	{
		public override string TypeName => "Iteration";
		public int Index { get; set; }

        public override void Setup()
        {
            foreach (var Item in Group.Items)
            {
                var Clone = Item.Clone(this);
                Clone.Iteration = this;
                Items.Add(Clone);
            }
        }

    }
}
