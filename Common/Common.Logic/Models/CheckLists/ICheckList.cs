using System.Collections.Generic;
using CheckListManager.Common.Logic.Models.Expressions;

namespace CheckListManager.Common.Logic.Models.CheckLists
{
    public interface ICheckList : IElementsBranch
    {
        Dictionary<string, Expression> Expressions { get; }

        void AddItem(Item item);

        Group Header { get; }

        List<Page> Pages { get; }
    }
}
