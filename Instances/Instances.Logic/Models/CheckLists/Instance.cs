using System;
using System.Collections.Generic;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions;
using Newtonsoft.Json;
using System.Linq;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic;
using CheckListManager.Common.Logic.Models;
using CheckListManager.Instances.Logic.Exceptions;
using CheckListManager.Common.Logic.Dependency;

namespace CheckListManager.Instances.Logic.Models.CheckLists
{
    public class Instance : ICheckList
    {
        public bool ReadOnly { get; set; }
        public Dictionary<string, ICheckListElement> Elements { get; set; } = new Dictionary<string, ICheckListElement>();
        public PascalCaseDictionary<string, Expression> Expressions { get; set; }
        public PascalCaseDictionary<string, string> Exceptions =>
            Expressions == null ? null : new PascalCaseDictionary<string, string>(Expressions.Where(e => e.Value?.Exception == true && e.Value?.RawValue as bool? == true).ToDictionary(k => k.Key, v => v.Value?.Description));
        public PascalCaseDictionary<string, string> ExternalSources { get; set; }
        public PascalCaseDictionary<string, PascalCaseDictionary<string, string>> ExternalSourceRows { get; set; }
        public ICheckListGroup Header { get; set; }
        public List<ICheckListPage> Pages { get; set; }

        public Formula DescriptionFormula { get; } = new Formula();
        public string DescriptionFormulaString { get; set; }
        public string Description => DescriptionFormula.Value;

        public Formula FileNameFormula { get; } = new Formula();
        public string FileNameFormulaString { get; set; }
        public string FileName => FileNameFormula.Value;

        public bool FTPExport { get; set; }

        [JsonIgnore]
        public bool InCreation { get; private set; }

        public ICheckList CheckList
        {
            get => this;
            set => throw new NotImplementedException("Just for inheritance purpose");
        }

        public decimal InstanceModelVersion { get; set; }
        private readonly decimal ModelVersion = 2.0m;
        public bool CheckVersion(decimal? ReferenceModelVersion, bool Save)
        {
            var Result = InstanceModelVersion >= (ReferenceModelVersion ?? ModelVersion);
            if (!Result && Save)
                InstanceModelVersion = ModelVersion;
            return Result;
        }

        private int itemSequence;
        public void AddItem(ICheckListItem item)
        {
            if (item.Id == 0 || InCreation)
                item.Id = ++itemSequence;
            else
                itemSequence = Math.Max(itemSequence, item.Id);

            Items.Add(item.Id, item);
        }

        public Dictionary<int, ICheckListItem> Items { get; } = new Dictionary<int, ICheckListItem>();

        public void Setup(bool creation)
        {
            //Setup must be called after deserialization
            InCreation = creation;

            if (creation)
                ExternalSourceRows = new PascalCaseDictionary<string, PascalCaseDictionary<string, string>>(ExternalSources.ToDictionary(x => x.Key, x => new PascalCaseDictionary<string, string>()));

            Header.CheckList = this;
            Header.Setup();
            if (InCreation) Header.CreateInstanceIterations();

            foreach (var page in Pages)
            {
                page.CheckList = this;
                page.Setup();
                if (InCreation) page.CreateInstanceIterations();
            }

            foreach (var expression in Expressions)
                expression.Value.Code = expression.Key;

            ElementsList = Elements.Values.Where(Element => !(Element is ICheckListItem CheckListItem) || !CheckListItem.IterationItem && !CheckListItem.IteratorModelItem).ToList(); //pages and items outside iterators

            if (InCreation || !CheckVersion(2, false)) //version 2 introduces dependencies
                BuildDependencies();
            InCreation = false;
        }

        public void BuildDependencies()
        {
            Updated = false;
            foreach (var Expression in Expressions.Values)
                Expression.ClearDependencies();

            DependingNodes.Clear();
            DependentNodes.Clear();

            ((Group)Header).ClearDependencies();
            int Index = 0;
            foreach (Page Page in Pages)
            {
                Page.Index = Index++;
                Page.ClearDependencies();
            }

            foreach (var Expression in Expressions.Values)
                if (!Expression.BuildDependencies(this))
                    throw new InstanceException(Expression.Error);

            DependencyNodeKey DependencyNodeKey = this.DependencyNodeKey.Clone(DependencyNodeKey.Processes.Other, "instance-description");
            DescriptionFormula.BuildDependencies(DescriptionFormulaString);
            foreach (var Key in DescriptionFormula.ValueKeys)
                if (Expressions.TryGetValue(Key, out var Expression))
                {
                    Expression.DependentNodes.Add(DependencyNodeKey);
                    DependingNodes.Add(Expression.DependencyNodeKey);
                }

            DependencyNodeKey = this.DependencyNodeKey.Clone(DependencyNodeKey.Processes.Other, "instance-file-name");
            FileNameFormula.BuildDependencies(FileNameFormulaString);
            foreach (var Key in FileNameFormula.ValueKeys)
                if (Expressions.TryGetValue(Key, out var Expression))
                {
                    Expression.DependentNodes.Add(DependencyNodeKey);
                    DependingNodes.Add(Expression.DependencyNodeKey);
                }

            ((Group)Header).BuildDependencies();
            foreach (Page Page in Pages)
                Page.BuildDependencies();
        }

        public string DecimalSeparator { get; set; }
        public string TimeZone { get; set; }
        public Dictionary<string, object> InstanceRuntimeValues { get; private set; } = new Dictionary<string, object>();
        public void UpdateInstanceRuntimeValues (InstanceCacheObject cacheObject)
        {
            InstanceRuntimeValues = new Dictionary<string, object> {
                { "InstanceCompany", cacheObject.CompanyId },
                { "InstanceDepartment", cacheObject.Department },
                { "InstanceDepartmentName", cacheObject.DepartmentName },
                { "InstancePrototypeType", cacheObject.PrototypeType },
                { "InstancePrototypeTypeName", cacheObject.PrototypeTypeName },
                { "InstancePrototype", cacheObject.Prototype },
                { "InstancePrototypeName", cacheObject.PrototypeName },
                { "InstanceVersion", cacheObject.Version },
                { "InstanceYear", cacheObject.YearCode },
                { "InstanceNumber", cacheObject.Number },
            };
        }


        private readonly Stack<string> DependencyChain = new Stack<string>();
        public void UpdateAll(string DecimalSeparator, string TimeZone, InstanceCacheObject cacheObject = null)
        {
            if (cacheObject != null) UpdateInstanceRuntimeValues(cacheObject);
            this.DecimalSeparator = DecimalSeparator;
            this.TimeZone = TimeZone;

            UpdateNodes(Expressions.Values, DependencyNodeKey.Processes.Other);

            /* traverse all to update the nodes not dependent by an expression */
            Update(false);
            ((Group)Header).Update();
            foreach (var page in Pages)
                ((Page)page).Update();
        }

        public void UpdateNodes(IEnumerable<IDependencyNode> Nodes, DependencyNodeKey.Processes Process)
        {
            foreach (var Node in Nodes)
            {
                DependencyChain.Clear();
                Node.TraverseDependencyChain(this, DependencyChain, Process, e => {
                    ((Expression)e).Evaluate(this, TimeZone);
                    return true;
                });
            }

        }

        public void UpdateReadOnly(string userCode, string state)
        {
            ((Group)Header).UpdateReadOnly(userCode, ReadOnly || state != InstanceState.Draft);
            foreach (var page in Pages)
                ((Page)page).UpdateReadOnly(userCode, ReadOnly || state != InstanceState.Progress);
        }

        public bool HideExpressions { get; set; }

        public bool ShouldSerializeExpressions()
        {
            return !HideExpressions;
        }

        [JsonIgnore]
        public List<ICheckListElement> ElementsList { get; private set; }

        public TypedValue GetExpressionValue(string ExpressionCode, ICheckListElement Element)
        {
            if (ExpressionCode == null || !CheckList.Expressions.ContainsKey(ExpressionCode)) return null;

            var IterationIndexes = (Element as ICheckListItem)?.Iteration?.Indexes;
            return GetExpressionValue(ExpressionCode, IterationIndexes);
        }

        public TypedValue GetExpressionValue(string ExpressionCode, List<int> IterationIndexes, out string Error)
        {
            Error = null;
            if (ExpressionCode == null || !CheckList.Expressions.ContainsKey(ExpressionCode))
            {
                Error = $"Expression {ExpressionCode} not found";
                return null;
            }

            var Value = CheckList.Expressions[ExpressionCode].Value;
            var Index = 0;
            while (Value?.IsNull == false && Value?.Type == DataType.Array)
            {
                List<TypedValue> ArrayValue;
                if (IterationIndexes == null)
                {
                    Error = $"The expression {ExpressionCode} returns an array";
                    return null;
                }
                if (IterationIndexes.Count <= Index)
                {
                    Error = $"The depth of this iteration is {IterationIndexes.Count} and is less than the depth of the array returned by the expression {ExpressionCode}";
                    return null;
                }
                if ((ArrayValue = Value.ArrayValue).Count() <= IterationIndexes[Index])
                {
                    Error = $"The expression {ExpressionCode} returns an array with less than {IterationIndexes[Index] + 1} values at level {Index}";
                    return null;
                }
                Value = ArrayValue.ToList()[IterationIndexes[Index]];
                ++Index;
            }
            return Value;
        }

        public TypedValue GetExpressionValue(string ExpressionCode, List<int> IterationIndexes) => GetExpressionValue(ExpressionCode, IterationIndexes, out var Error);

        public DependencyNodeKey DependencyNodeKey => new DependencyNodeKey("checklist", null);
        public HashSet<DependencyNodeKey> DependentNodes { get; } = new HashSet<DependencyNodeKey>();
        public HashSet<DependencyNodeKey> DependingNodes { get; } = new HashSet<DependencyNodeKey>();

        public bool TraverseDependencyChain(ICheckList checkList, Stack<string> DependencyChain, DependencyNodeKey.Processes Process, Func<IDependencyNode, bool> DependencyChainAction)
        {
            if (Updated) return true;

            foreach (var DependencyNodeKey in DependingNodes)
                DependencyNodeKey.GetNode(checkList).TraverseDependencyChain(checkList, DependencyChain, Process, DependencyChainAction);

            Update(false);
            return true;
        }

        public void MarkNodeAsChanged(ICheckList CheckList, List<IDependencyNode> Roots, DependencyNodeKey.Processes Process)
        {
            if (!Updated) return;
            Updated = false;
            if (DependentNodes.Any())
                foreach (var DependencyNodeKey in DependentNodes)
                    DependencyNodeKey.GetNode(CheckList).MarkNodeAsChanged(CheckList, Roots, Process);
            else
                Roots.Add(this);
        }

        private bool Updated { get; set; } = true;

        public void Update(bool Force)
        {
            if (Updated && !Force) return;

            DescriptionFormula.Evaluate(this, InstanceRuntimeValues, DecimalSeparator, TimeZone, null);
            FileNameFormula.Evaluate(this, InstanceRuntimeValues, DecimalSeparator, TimeZone, null);
            Updated = true;

        }

    }
}
