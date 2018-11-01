using System.Collections.Generic;
using System.Linq;
using CheckListManager.Common.Logic.Models.CheckLists;
using System;
using CheckListManager.Instances.Logic.Exceptions;

namespace CheckListManager.Instances.Logic.Models.CheckLists
{
	public class Group : Item, ICheckListGroup
	{
		private bool _valid;

		public override string TypeName => "Group";

		public virtual Dictionary<string, ICheckListElement> Elements { get; set; }

		public List<ICheckListItem> Items { get; set; } = new List<ICheckListItem>();
        public List<ICheckListIteration> Iterations { get; } = new List<ICheckListIteration>();
		public bool Iterator { get; set; }

        public bool ShowAsTable { get; set; }

        public int InstanceIterations { get; set; }

        public int PrototypeIterations { get; set; }

        public bool DisableAdd { get; set; }

        public bool DisableRemove { get; set; }

        public bool DisableMove { get; set; }

        public override bool IsValidByDefault() => _valid;

        public override bool Hidden => (Page?.Hidden ?? false) || base.Hidden;
        
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

                if (((Instance)CheckList).InCreation && InstanceIterations > 0 && !IteratorModelItem)
                    for (int Index = 0; Index < InstanceIterations; ++Index)
                        AddIteration();

                Elements = new Dictionary<string, ICheckListElement>();
			}
			else
				Elements = Items
					.Where(item => item.Code != null && item.Code != string.Empty)
					.ToDictionary(item => item.Code, item => (ICheckListElement)item);
		}

		public override void Update(string DecimalSeparator)
		{
			base.Update(DecimalSeparator);

			_valid = true;
			foreach (var item in Items)
			{
				item.Update(DecimalSeparator);
				_valid = _valid && item.Valid;
			}

			var index = 1;
			if (Iterator)
			{
				_valid = true;
				foreach (var iteration in Iterations)
				{
					iteration.Index = index++;
					iteration.Update(DecimalSeparator);
					_valid = _valid && (iteration.Removed || iteration.Valid);
				}
			}
		}

		public override void UpdateReadOnly(string userCode, bool readOnlyInstance)
		{
			base.UpdateReadOnly(userCode, readOnlyInstance);

			if (Iterator)
				foreach (var iteration in Iterations)
					iteration.UpdateReadOnly(userCode, readOnlyInstance);
            else
                foreach (var item in Items)
                    item.UpdateReadOnly(userCode, readOnlyInstance);

        }

        public override ICheckListItem Clone(ICheckListGroup parent)
		{
			var group = new Group
			{
				CheckList = CheckList,
				Iterator = Iterator,
                ShowAsTable = ShowAsTable,
                InstanceIterations = InstanceIterations,
                PrototypeIterations = PrototypeIterations,
                DisableAdd = DisableAdd,
                DisableRemove = DisableRemove,
                DisableMove = DisableMove,
                Group = parent
			};
			foreach (var item in Items)
				group.Items.Add(item.Clone(group));

			Copy(this, group);
			return group;
		}

        private void CheckSignatures()
        {
            foreach (var Item in Items)
                if (Item is Group Group)
                    Group.CheckSignatures();
                else if (Item is Signature && ((Signature)Item).Signed)
                    throw new InstanceException("The iteration contains issued signatures and cannot be removed");

            if (Iterator)
                foreach (var Iteration in Iterations)
                    ((Iteration)Iteration).CheckSignatures();
        }

        public override void Remove()
		{
            CheckSignatures();

            foreach (var item in Items)
				item.Remove();

			if (Iterator)
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

        public Iteration AddIteration()
        {
            var iteration = new Iteration
            {
                CheckList = CheckList,
                Page = Page,
                Group = this
            };
            iteration.Setup();
            Iterations.Add(iteration);
            return iteration;
        }

		public void Move(Iteration iteration, int direction)
		{
            int 
                from = Iterations.IndexOf(iteration),
                to = from + direction;
            while (Iterations[to].Removed)
                to += direction;
            Iterations.RemoveAt(from);
            Iterations.Insert(to, iteration);
		}

		public void Remove(Iteration iteration)
		{
			iteration.Remove();
        }
    }
}
