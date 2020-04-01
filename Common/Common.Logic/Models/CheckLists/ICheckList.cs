using System.Collections.Generic;
using CheckListManager.Common.Logic.Dependency;
using CheckListManager.Common.Logic.Models.Expressions;
using Newtonsoft.Json;

namespace CheckListManager.Common.Logic.Models.CheckLists
{
    public interface ICheckList : IElementsBranch, IDependencyNode
    {
        string DescriptionFormulaString { get; set; }
        string FileNameFormulaString { get; set; }
        bool FTPExport { get; set; }

        PascalCaseDictionary<string, Expression> Expressions { get; }
        PascalCaseDictionary<string, string> ExternalSources { get; }
        PascalCaseDictionary<string, PascalCaseDictionary<string, string>> ExternalSourceRows { get; }

        [JsonIgnore]
        Dictionary<int, ICheckListItem> Items { get; }
        void AddItem(ICheckListItem item);

        ICheckListGroup Header { get; }

        List<ICheckListPage> Pages { get; }

        TypedValue GetExpressionValue(string ExpressionCode, ICheckListElement Element);
        TypedValue GetExpressionValue(string ExpressionCode, List<int> IterationIndexes);
        TypedValue GetExpressionValue(string ExpressionCode, List<int> IterationIndexes, out string Error);
    }
}
