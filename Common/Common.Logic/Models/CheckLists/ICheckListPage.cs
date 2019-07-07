using System;
using System.Collections.Generic;

namespace CheckListManager.Common.Logic.Models.CheckLists
{
	public interface ICheckListPage : ICheckListElement
	{
		string Description { get; set; }
		List<ICheckListGroup> Groups { get; set; }
		void Traverse(Func<ICheckListItem,bool> condition, Action<ICheckListItem> action);
	}
}