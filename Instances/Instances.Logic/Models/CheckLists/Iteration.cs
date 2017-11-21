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
			var loading = Id > 0;
			CheckList.AddItem(this);
			foreach (var item in Group.Items)
			{
				var clone = item.Clone(this);
				clone.Iteration = this;
				if (!loading)
					Items.Add(clone);
			}
			SetupItems();

			IEnumerable<ICheckListElement> availableItems = Items.Where(x => x.Code != null);

			var node = Group;
			while (node != null)
			{
				if (!node.IterationItem)
					node = null;
				else
				{
					if (node.Elements != null)
						availableItems = availableItems.Concat(node.Elements.Values);
					node = node.Group;
				}
			}
			availableItems = availableItems.Concat(CheckList.Elements.Values.Where(element => !(element is ICheckListItem) || !((ICheckListItem)element).IterationItem));

			_elements = availableItems
				.ToDictionary(item => item.Code, item => item);
		}

		private Dictionary<string, ICheckListElement> _elements;
		public override Dictionary<string, ICheckListElement> Elements => _elements;
	}
}
