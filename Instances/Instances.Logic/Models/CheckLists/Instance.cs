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
        [JsonIgnore]
        public int DependencyChainTraverseIndex { get; set; } = 1;

        public ICheckList CheckList
        {
            get => this;
            set => throw new NotImplementedException("Just for inheritance purpose");
        }

        public decimal InstanceModelVersion { get; set; }
        private readonly decimal ModelVersion = 2.0m;
        public bool CheckVersion(bool Save)
        {
            var Result = InstanceModelVersion == ModelVersion;
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
            {
                expression.Value.Code = expression.Key;
                expression.Value.Parse(this);
            }

            if (ElementsList == null) //if (InCreation) //replaced with this more generic but less meaningful form for back compatibility with 1.4 when ElementList did not exist
                ElementsList = Elements.Values.Where(Element => !(Element is ICheckListItem CheckListItem) || !CheckListItem.IterationItem && !CheckListItem.IteratorModelItem).ToList(); //pages and items outside iterators

            if (!CheckVersion(false))
                BuildDependencies();
            InCreation = false;
        }

        public void BuildDependencies()
        {
            foreach(var Expression in Expressions.Values)
                Expression.ClearDependencies();

            var DependencyNodeKey = this.DependencyNodeKey.Clone("instance-description");
            DescriptionFormula.BuildDependencies(DescriptionFormulaString);
            foreach (var Key in DescriptionFormula.ValueKeys)
                if (Expressions.TryGetValue(Key, out var Expression))
                {
                    Expression.DependentNodes.Add(DependencyNodeKey);
                    DependingNodes.Add(Expression.DependencyNodeKey);
                }

            DependencyNodeKey = this.DependencyNodeKey.Clone("instance-file-name");
            FileNameFormula.BuildDependencies(FileNameFormulaString);
            foreach (var Key in FileNameFormula.ValueKeys)
                if (Expressions.TryGetValue(Key, out var Expression))
                {
                    Expression.DependentNodes.Add(DependencyNodeKey);
                    DependingNodes.Add(Expression.DependencyNodeKey);
                }

            foreach (var Expression in Expressions.Values)
                if (!Expression.BuildDependencies(this))
                    throw new InstanceException(Expression.Error);

            ((Group)Header).BuildDependencies();
            foreach (Page Page in Pages)
                Page.BuildDependencies();
        }

        [JsonIgnore]
        public string DecimalSeparator { get; private set; }
        [JsonIgnore]
        public string TimeZone { get; private set; }
        public Dictionary<string, object> InstanceRuntimeValues { get; private set; }

        private readonly Stack<IDependencyNode> DependencyChain = new Stack<IDependencyNode>();
        private string DependencyChainKey;
        public void Update(string DecimalSeparator, string TimeZone, InstanceCacheObject cacheObject = null)
        {
            this.DecimalSeparator = DecimalSeparator;
            this.TimeZone = TimeZone;
            InstanceRuntimeValues = cacheObject == null ? new Dictionary<string, object>() : new Dictionary<string, object> {
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

            foreach (var Expression in Expressions.Values)
            {
                DependencyChain.Clear();
                DependencyChainKey = Expression.Code;
                Expression.TraverseDependencyChain(this, DependencyChain, DependencyChainKey, e => {
                    ((Expression)e).Evaluate(this, TimeZone);
                    return true;
                });
            }

            DescriptionFormula.Evaluate(this, InstanceRuntimeValues, DecimalSeparator, TimeZone, null);
            FileNameFormula.Evaluate(this, InstanceRuntimeValues, DecimalSeparator, TimeZone, null);

            Header.Update(DecimalSeparator, TimeZone);
            foreach (var page in Pages)
                page.Update(DecimalSeparator, TimeZone);
        }

        public void UpdateReadOnly(string userCode, string state)
        {
            Header.UpdateReadOnly(userCode, ReadOnly || state != InstanceState.Draft);
            foreach (var page in Pages)
                page.UpdateReadOnly(userCode, ReadOnly || state != InstanceState.Progress);
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
                IEnumerable<TypedValue> ArrayValue;
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

        public bool TraverseDependencyChain(ICheckList checkList, Stack<IDependencyNode> DependencyChain, string DependencyChainKey, Func<IDependencyNode, bool> DependencyChainAction)
        {
            DescriptionFormula.Evaluate(this, InstanceRuntimeValues, DecimalSeparator, TimeZone, null);
            FileNameFormula.Evaluate(this, InstanceRuntimeValues, DecimalSeparator, TimeZone, null);

            return true;
        }

    }
}
