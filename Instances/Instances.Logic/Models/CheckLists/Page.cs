using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Instances.Logic.Models.CheckLists
{
	public class Page : Element, ICheckListPage
	{
		private bool _valid;

		public string Description { get; set; }
		public List<ICheckListGroup> Groups { get; set; } = new List<ICheckListGroup>();

		public Lock Lock { get; set; }

        public override bool IsValidByDefault()
			=> _valid;

        public void CreateInstanceIterations()
        {
            foreach (var Group in Groups)
                Group.CreateInstanceIterations();
        }

        public override void Setup()
		{
			base.Setup();

			ICheckListGroup previous = null;
			foreach (var group in Groups)
			{
				group.CheckList = CheckList;
				group.Page = this;
				group.Previous = previous;
				group.Setup();
				previous = group;
			}
		}

		public override void Update(string DecimalSeparator, string TimeZone)
		{
			_valid = true;
			foreach (var group in Groups)
			{
				group.Update(DecimalSeparator, TimeZone);
				_valid = _valid && group.Valid;
			}
		}

		public override void UpdateReadOnly(string userCode, bool ReadOnlyInstance)
		{
            if (Lock?.User != userCode || ReadOnlyInstance)
                ReadOnly = true;
            else
                base.UpdateReadOnly(userCode, ReadOnlyInstance);

            foreach (var group in Groups)
				group.UpdateReadOnly(userCode, ReadOnlyInstance);
		}

        public void Traverse(Func<ICheckListItem, bool> condition, Action<ICheckListItem> action)
        {
            throw new NotImplementedException();
        }

        public Task TraverseAsync(Func<ICheckListItem, bool> condition, Func<ICheckListItem, Task> action)
        {
            throw new NotImplementedException();
        }
    }
}
