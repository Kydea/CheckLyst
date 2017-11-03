using System;
using System.Collections.Generic;
using System.Linq;

namespace CheckListManager.Common.Logic.Models.CheckLists
{
	public class Iteration : Group, ICheckListIteration
	{
		public override string TypeName => "Iteration";

		public override Item Clone(Group parent)
		{
			throw new NotImplementedException("Why? Clone the parent group instead.");
		}

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
          
            IEnumerable<Element> availableItems = Items.Where(x => x.Code != null);

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
			availableItems = availableItems.Concat(CheckList.Elements.Values.Where(element => !(element is Item) || !((Item)element).IterationItem));

			_elements = availableItems.ToDictionary(item => item.Code, item => item);
		}

		private Dictionary<string, Element> _elements;
		public override Dictionary<string, Element> Elements => _elements;

		public int Index { get; set; }

		public override dynamic GetModel(uint clientId)
		{
			var result = base.GetModel(clientId);
			result.Index = Index;
			return result;
		}
    }
}
