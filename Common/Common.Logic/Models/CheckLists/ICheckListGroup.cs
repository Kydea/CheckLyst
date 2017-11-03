using System.Collections.Generic;

namespace CheckListManager.Common.Logic.Models.CheckLists
{
	public interface ICheckListGroup : ICheckListItem, IElementsBranch
	{
		List<ICheckListItem> Items { get; set; }
		List<ICheckListIteration> Iterations { get; }
		bool Iterator { get; set; }
	}
}