using CheckListManager.Common.Logic.Models.Expressions;
using System.Collections.Generic;

namespace CheckListManager.Common.Logic.Models.CheckLists
{
	public interface ICheckListIteration : ICheckListGroup, IElementsBranch
    {
		int Index { get; set; }
		int Identifier { get; set; }
        List<int> Indexes { get; set; }
        TypedValue ReccurencyValue { get; set; }
    }
}