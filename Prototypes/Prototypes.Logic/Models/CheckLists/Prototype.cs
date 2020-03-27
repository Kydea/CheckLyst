using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CheckListManager.Common.Logic;
using CheckListManager.Common.Logic.Dependency;
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
        public Exception Exception { get; set; }

        public Dictionary<int, ICheckListItem> Items { get; } = new Dictionary<int, ICheckListItem>();
        public void AddItem(ICheckListItem item)
        {
            Items.Add(item.Id, item);
        }

        public TypedValue GetExpressionValue(string ExpressionCode, ICheckListElement Element)
        {
            throw new NotImplementedException(); //only for instances
        }

        public TypedValue GetExpressionValue(string ExpressionCode, List<int> IterationIndexes) { throw new NotImplementedException("Just for inheritance purpose"); }
        public TypedValue GetExpressionValue(string ExpressionCode, List<int> IterationIndexes, out string Error) { throw new NotImplementedException("Just for inheritance purpose"); }

        public ICheckList CheckList { get { return this; } set { throw new NotImplementedException("Just for inheritance purpose"); } }

        private readonly Stack<IDependencyNode> DependencyChain = new Stack<IDependencyNode>();
        private string DependencyChainKey;

        public void Setup()
        {
            try
            {
                Exception = null;
                Header.CheckList = this;
                Header.Setup();
                foreach (var Page in Pages)
                {
                    Page.CheckList = this;
                    Page.Setup();
                }

                bool Error = false;
                foreach (var expression in Expressions)
                    try
                    {
                        if (expression.Key.Length > 50)
                            throw new Exception($"Expression codes must not exceed 50 characters");

                        if (expression.Value.Description?.Length > 100)
                            throw new Exception($"Expression descriptions must not exceed 100 characters");

                        expression.Value.Validate(this, expression.Key);
                        expression.Value.Parse(this);
                    }
                    catch (ParseException Exception)
                    {
                        expression.Value.Error = $"{Exception.Message} (position {Exception.Position})";
                        Error = true;
                    }
                    catch (Exception Exception)
                    {
                        expression.Value.Error = Exception.Message;
                        Error = true;
                    }

                if (Error)
                    throw new Exception("Some expression is not valid");

                foreach (var expression in Expressions.Values)
                    if (expression.Error == null && !expression.BuildDependencies(this))
                        Error = true;

                if (Error)
                    throw new Exception("Some expression is not valid");

                ((Group)Header).BuildDependencies();
                foreach (Page Page in Pages)
                    Page.BuildDependencies();

                foreach (var expression in Expressions.Values)
                {
                    DependencyChain.Clear();
                    DependencyChainKey = expression.Code;
                    if (expression.Error == null && !expression.TraverseDependencyChain(this, DependencyChain, DependencyChainKey, dn => ((Expression)dn).Check(this)))
                        Error = true;
                }

                if (Error)
                    throw new Exception("Some expression is not valid");

                ((Group)Header).Check();
                foreach (Page Page in Pages)
                    Page.Check();

                foreach (var expression in Expressions.Values)
                    if (expression.Error == null && !expression.Check(this))
                        Error = true;

                if (Error)
                    throw new Exception("Some expression is not valid");
            }
            catch (Exception Exception)
            {
                this.Exception = Exception;
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
