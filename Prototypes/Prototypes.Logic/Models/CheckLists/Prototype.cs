using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CheckListManager.Common.Logic;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;

namespace CheckListManager.Prototypes.Logic.Models.CheckLists
{
    public class Prototype : ICheckList
    {
        public Dictionary<string, ICheckListElement> Elements { get; } = new Dictionary<string, ICheckListElement>();
        public PascalCaseDictionary<string, Expression> Expressions { get; set; } = new PascalCaseDictionary<string, Expression>();
        public PascalCaseDictionary<string, string> ExternalSources { get; set; } = new PascalCaseDictionary<string, string>();
        public PascalCaseDictionary<string, PascalCaseDictionary<string, string>> ExternalSourceRows { get; }
        public ICheckListGroup Header { get; set; }
        public List<ICheckListPage> Pages { get; } = new List<ICheckListPage>();
        public string DescriptionFormulaString { get; set; }
        public string FileNameFormulaString { get; set; }
        public bool FTPExport { get; set; }

        public void AddItem(ICheckListItem item)
        {
            throw new NotImplementedException(); //only for instances
        }

        public TypedValue GetExpressionValue(string ExpressionCode, ICheckListElement Element)
        {
            throw new NotImplementedException(); //only for instances
        }

        public TypedValue GetExpressionValue(string ExpressionCode, List<int> IterationIndexes) { throw new NotImplementedException("Just for inheritance purpose"); }
        public TypedValue GetExpressionValue(string ExpressionCode, List<int> IterationIndexes, out string Error) { throw new NotImplementedException("Just for inheritance purpose"); }

        public ICheckList CheckList { get { return this; } set { throw new NotImplementedException("Just for inheritance purpose"); } }

        public void Setup()
        {
            Header.CheckList = this;
            Header.Setup();

            foreach (var Page in Pages)
            {
                Page.CheckList = this;
                Page.Setup();
            }

            foreach (var Expression in Expressions)
            {
                try
                {
                    if (Expression.Key.Length > 50)
                        throw new Exception($"Expression codes must not exceed 50 characters");

                    if (Expression.Value.Description?.Length > 100)
                        throw new Exception($"Expression descriptions must not exceed 100 characters");

                    Expression.Value.Validate(this);
                    Expression.Value.Parse(this);
                    Expression.Value.Check(this);
                }
                catch (ParseException Exception)
                {
                    Expression.Value.Error = $"{Exception.Message} (position {Exception.Position})";
                }
                catch (Exception Exception)
                {
                    Expression.Value.Error = Exception.Message;
                }
            }
        }

        public void Traverse(Func<ICheckListItem, bool> condition, Action<ICheckListItem> action)
        {
            Header.Traverse(condition, action);

            foreach (var page in Pages)
                page.Traverse(condition, action);
        }

        public async Task TraverseAsync(Func<ICheckListItem, bool> condition, Func<ICheckListItem, Task> action)
        {
            await Header.TraverseAsync(condition, action);

            foreach (var page in Pages)
                await page.TraverseAsync(condition, action);
        }

    }
}
