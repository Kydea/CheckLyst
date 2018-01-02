using System.Collections.Generic;
using System.Linq;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Instances.Logic.Models.CheckLists
{
	public class Iteration : Group, ICheckListIteration
	{
		public override string TypeName => "Iteration";
		public int Index { get; set; }

		public override void Setup()
		{
			var Loading = Id > 0;
			CheckList.AddItem(this);
            if (Loading)
                foreach (var Item in Items)
                {
                    Item.Iteration = this;
                    Item.IterationItem = true;
                }
            else
                foreach (var Item in Group.Items)
                {
                    var Clone = Item.Clone(this);
                    Clone.Iteration = this;
                    Items.Add(Clone);
                }
            SetupItems();

			IEnumerable<ICheckListElement> AvailableItems = Items.Where(x => x.Code != null);

			var node = Group;
			while (node != null)
			{
				if (!node.IterationItem)
					node = null;
				else
				{
					if (node.Elements != null)
						AvailableItems = AvailableItems.Concat(node.Elements.Values);
					node = node.Group;
				}
			}
            AvailableItems = AvailableItems.Concat(CheckList.Elements.Values.Where(Element => !(Element is ICheckListItem) || !((ICheckListItem)Element).IterationItem && !((ICheckListItem)Element).IteratorModelItem));

            _elements = AvailableItems
				.ToDictionary(item => item.Code, item => item);
		}

		private Dictionary<string, ICheckListElement> _elements;
		public override Dictionary<string, ICheckListElement> Elements => _elements;
	}
}
