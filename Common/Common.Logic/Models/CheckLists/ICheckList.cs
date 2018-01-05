using System.Collections.Generic;
using CheckListManager.Common.Logic.Models.Expressions;
using Newtonsoft.Json;

namespace CheckListManager.Common.Logic.Models.CheckLists
{
    public interface ICheckList : IElementsBranch
    {
        string DescriptionFormulaString { get; set; }
        string FileNameFormulaString { get; set; }
        bool FTPExport { get; set; }

        Dictionary<string, Expression> Expressions { get; }

        void AddItem(ICheckListItem item);

        ICheckListGroup Header { get; }

        List<ICheckListPage> Pages { get; }
    }
}
