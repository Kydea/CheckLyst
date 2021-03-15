using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CheckListManager.Common.Logic;
using CheckListManager.Common.Logic.Dependency;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;
using Newtonsoft.Json;

namespace CheckListManager.Prototypes.Logic.Models.CheckLists
{
    public class Prototype : DependencyNode, ICheckList
    {
        public Dictionary<string, ICheckListElement> Elements { get; } = new Dictionary<string, ICheckListElement>();
        public PascalCaseDictionary<string, Expression> Expressions { get; } = new PascalCaseDictionary<string, Expression>();
        public PascalCaseDictionary<string, string> ExternalSources { get; } = new PascalCaseDictionary<string, string>();
        public PascalCaseDictionary<string, PascalCaseDictionary<string, string>> ExternalSourceRows { get; }
        public PascalCaseDictionary<string, ExternalSourceDependencyNode> ExternalSourceDependencyNodes { get; } = new PascalCaseDictionary<string, ExternalSourceDependencyNode>();
        public ICheckListGroup Header { get; set; }
        public List<ICheckListPage> Pages { get; } = new List<ICheckListPage>();
        public string DescriptionFormulaString { get; set; }
        private Formula DescriptionFormula { get; } = new Formula();
        public string FileNameFormulaString { get; set; }
        private Formula FileNameFormula { get; } = new Formula();
        private bool PreserveID { get; set; }
        
        public bool FTPExport { get; set; }
        public Exception Exception { get; set; }

        public bool NeedToSaveExpressionValues { get; set; }

        [JsonProperty]
        private int ItemIDSequence { get; set; }
        public Dictionary<int, ICheckListItem> Items { get; } = new Dictionary<int, ICheckListItem>();
        public void AddItem(ICheckListItem item)
        {
            if (PreserveID)
                ItemIDSequence = Math.Max(item.Id + 1, ItemIDSequence);
            else
                item.Id = ItemIDSequence++;
            Items.Add(item.Id, item);
        }

        public TypedValue GetExpressionValue(string ExpressionCode, ICheckListElement Element)
        {
            throw new NotImplementedException(); //only for instances
        }

        public TypedValue GetExpressionValue(string ExpressionCode, List<int> IterationIndexes) { throw new NotImplementedException("Just for inheritance purpose"); }
        public TypedValue GetExpressionValue(string ExpressionCode, List<int> IterationIndexes, out string Error) { throw new NotImplementedException("Just for inheritance purpose"); }

        public ICheckList CheckList { get { return this; } set { throw new NotImplementedException("Just for inheritance purpose"); } }

        public void Setup(bool PreserveID)
        {
            try
            {
                Exception = null;
                this.PreserveID = PreserveID;
                if (!PreserveID) ItemIDSequence = 1;
                Header.CheckList = this;
                ((Group)Header).Setup();
                int Index = 0;
                foreach (Page Page in Pages)
                {
                    if (!PreserveID)
                        Page.Index = Index++;
                    Page.CheckList = this;
                    Page.Setup();
                }

                ExternalSourceDependencyNodes.Clear();
                foreach (string ExternalSource in ExternalSources.Keys)
                    ExternalSourceDependencyNodes.Add(ExternalSource, new ExternalSourceDependencyNode { Code = ExternalSource });

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
                        expression.Value.ClearDependencies();
                        expression.Value.DependencyChainAction = e => e.Check(this);
                    }
                    catch (ParseException Exception)
                    {
                        expression.Value.Error = Exception.Message;
                        expression.Value.ErrorPosition = Exception.Position;
                        Error = true;
                    }
                    catch (Exception Exception)
                    {
                        expression.Value.Error = Exception.Message;
                        Error = true;
                    }

                if (Error || !BuildDependencies())
                    throw new Exception("Some expressions are not valid");


                foreach (var expression in Expressions.Values)
                    if (expression.Error == null)
                    {
                        expression.TestDependencyChain(this, DependencyRelation.Properties.Value);
                        Error |= expression.Error != null;
                    }

                if (Error)
                    throw new Exception("Some expressions are not valid");

                ((Group)Header).Check();
                foreach (Page Page in Pages)
                    Page.Check();

                foreach (var expression in Expressions.Values)
                    if (expression.Error == null && !expression.Check(this))
                        Error = true;

                if (Error)
                    throw new Exception("Some expressions are not valid");
            }
            catch (Exception Exception)
            {
                this.Exception = Exception;
            }
        }

        public override bool Update(DependencyRelation.Properties Property) => true;

        private bool BuildDependencies()
        {
            var Result = true;
            foreach (var expression in Expressions.Values)
                if (expression.Error == null && !expression.BuildDependencies(this))
                    Result = false;

            if (Result)
            {
                ClearDependencies();

                DescriptionFormula.BuildDependencies(DescriptionFormulaString, null);
                foreach (var Key in DescriptionFormula.ValueKeys)
                    if (Expressions.TryGetValue(Key, out var Expression))
                        AddDependingRelation(DependencyRelation.Properties.Formula, Expression, DependencyRelation.Properties.Value);

                FileNameFormula.BuildDependencies(FileNameFormulaString, null);
                foreach (var Key in FileNameFormula.ValueKeys)
                    if (Expressions.TryGetValue(Key, out var Expression))
                        AddDependingRelation(DependencyRelation.Properties.Formula, Expression, DependencyRelation.Properties.Value);

                ((Group)Header).BuildDependencies();
                foreach (Page Page in Pages)
                    Page.BuildDependencies();
            }

            return Result;
        }

        public override string DependencyClassName => "checklist";

        public void Traverse(Func<ICheckListItem, bool> condition, Action<ICheckListItem> action)
        {
            ((Group)Header).Traverse(condition, action);

            foreach (Page page in Pages)
                page.Traverse(condition, action);
        }

        public async Task TraverseAsync(Func<ICheckListItem, bool> condition, Func<ICheckListItem, Task> action)
        {
            await ((Group)Header).TraverseAsync(condition, action);

            foreach (Page page in Pages)
                await page.TraverseAsync(condition, action);
        }
    }
}
