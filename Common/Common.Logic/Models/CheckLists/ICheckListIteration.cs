using System.Collections.Generic;

namespace CheckListManager.Common.Logic.Models.CheckLists
{
	public interface ICheckListIteration : ICheckListGroup, IElementsBranch
    {
		int Index { get; set; }
        List<int> Indexes { get; set; }
    }
}