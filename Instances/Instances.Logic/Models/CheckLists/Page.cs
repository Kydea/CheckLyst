using System.Collections.Generic;
using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Instances.Logic.Models.CheckLists
{
	public class Page : Element, ICheckListPage
	{
		private bool _valid;

		public string Description { get; set; }
		public List<ICheckListGroup> Groups { get; set; } = new List<ICheckListGroup>();

		public override bool IsValidByDefault()
			=> _valid;

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

		public override void Update()
		{
			_valid = true;
			foreach (var group in Groups)
			{
				group.Update();
				_valid = _valid && group.Valid;
			}
		}

		public override void UpdateReadOnly(string userCode, bool canEdit)
		{
			base.UpdateReadOnly(userCode, canEdit);

			foreach (var group in Groups)
			{
				group.UpdateReadOnly(userCode, canEdit);
			}
			//TODO: update when locking is done
		}
	}
}
