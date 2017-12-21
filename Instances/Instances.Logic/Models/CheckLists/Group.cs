using System.Collections.Generic;
using System.Linq;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Instances.Logic.Models.CheckLists
{
	public class Group : Item, ICheckListGroup
	{
		private bool _valid;

		public override string TypeName => "Group";

		public virtual Dictionary<string, ICheckListElement> Elements { get; set; }

		public List<ICheckListItem> Items { get; set; }
		public List<ICheckListIteration> Iterations { get; } = new List<ICheckListIteration>();
		public bool Iterator { get; set; }

		public override bool IsValidByDefault()
			=> _valid;

		public override void Setup()
		{
			base.Setup();
			SetupItems();

			if (Iterator)
			{
				foreach (var iteration in Iterations)
				{
					iteration.CheckList = CheckList;
					iteration.Page = Page;
					iteration.Group = this;
					iteration.Setup();
				}
				Elements = new Dictionary<string, ICheckListElement>();
			}
			else
				Elements = Items
					.Where(item => item.Code != null)
					.ToDictionary(item => item.Code, item => (ICheckListElement)item);
		}

		public override void Update()
		{
			base.Update();

			_valid = true;
			foreach (var item in Items)
			{
				item.Update();
				_valid = _valid && item.Valid;
			}

			var index = 1;
			if (Iterator)
			{
				_valid = true;
				foreach (var iteration in Iterations)
				{
					iteration.Index = index++;
					iteration.Update();
					_valid = _valid && iteration.Valid;
				}
			}
		}

		public override void UpdateReadOnly(string userCode, bool canEdit)
		{
			base.UpdateReadOnly(userCode, canEdit);

			foreach (var item in Items)
			{
				item.UpdateReadOnly(userCode, canEdit);
			}

			if (Iterator)
			{
				_valid = true;
				foreach (var iteration in Iterations)
				{
					iteration.UpdateReadOnly(userCode, canEdit);
				}
			}
		}

		public override ICheckListItem Clone(ICheckListGroup parent)
		{
			var group = new Group
			{
				CheckList = CheckList,
				Iterator = Iterator,
				Group = parent
			};
			foreach (var item in Items)
				group.Items.Add(item.Clone(group));

			Copy(this, group);
			return group;
		}

		public override void Remove()
		{
			foreach (var item in Items)
				item.Remove();

			if (Group.Iterator)
				foreach (var iteration in Iterations)
					iteration.Remove();

			base.Remove();
		}

		protected void SetupItems()
		{
			ICheckListItem previous = null;

			foreach (var item in Items)
			{
				item.CheckList = CheckList;
				item.Page = Page;
				item.Group = this;
				item.Previous = previous;
				item.IteratorModelItem = Iterator || IteratorModelItem;
				item.Iteration = item.Iteration ?? Iteration;
				item.Setup();
				previous = item;
			}
		}

		public void AddIteration()
		{
			var iteration = new Iteration
			{
				CheckList = CheckList,
				Page = Page,
				Group = this
			};
			iteration.Setup();
			Iterations.Add(iteration);
		}

		public void Move(Iteration iteration, int direction)
		{
			var index = Iterations.IndexOf(iteration);
			Iterations.RemoveAt(index);
			Iterations.Insert(index + direction, iteration);
		}

		public void Remove(Iteration iteration)
		{
			Iterations.Remove(iteration);
			iteration.Remove();
		}
	}
}
