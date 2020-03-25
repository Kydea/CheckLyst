using System;
using System.Collections.Generic;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions;
using Newtonsoft.Json;
using System.Linq;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic;
using CheckListManager.Common.Logic.Models;

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

        [JsonIgnore]
        public Formula DescriptionFormula { get; set; }
        public string DescriptionFormulaString
        {
            get => DescriptionFormula?.RawValue;
            set => DescriptionFormula = value == null ? null : new Formula(value);
        }
        public string Description => DescriptionFormula?.Value;

        public bool DependenciesCreated { get; set; }

        [JsonIgnore]
        public Formula FileNameFormula { get; set; }
        public string FileNameFormulaString
        {
            get => FileNameFormula?.RawValue;
            set => FileNameFormula = value == null ? null : new Formula(value);
        }
        public string FileName => FileNameFormula?.Value;

        public bool FTPExport { get; set; }

        [JsonIgnore]
        public bool InCreation { get; private set; }

        public ICheckList CheckList
        {
            get => this;
            set => throw new NotImplementedException("Just for inheritance purpose");
        }
        
        public void AddItem(ICheckListItem item)
        {
            if (item.Id == 0 || InCreation)
                item.Id = ++_sequence;
            else
                _sequence = Math.Max(_sequence, item.Id);

            Items.Add(item.Id, item);
        }

        private int _sequence;

        [JsonIgnore]
        public Dictionary<int, ICheckListItem> Items { get; } = new Dictionary<int, ICheckListItem>(10);

        public void Setup(bool Creation)
        {
            //this must be called after deserialization
            InCreation = Creation;

            if (Creation)
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

            foreach (var expression in Expressions.Values)
                expression.Parse(this);

            var InstanceElements = CheckList.Elements.Values.Where(Element => !(Element is ICheckListItem) || !((ICheckListItem)Element).IterationItem && !((ICheckListItem)Element).IteratorModelItem);

            if (ElementsList == null) //if (InCreation) //replaced with this more generic but less meaningful form for back compatibility with 1.4 when ElementList did not exist
                ElementsList = Elements.Values.Where(Element => !(Element is ICheckListItem CheckListItem) || !CheckListItem.IterationItem && !CheckListItem.IteratorModelItem).ToList(); //pages and items outside iterators

            InCreation = false;
        }

        public void CreateDependencies()
        {
            //for back compatibility with 2.0.0 when dependency was not present
            foreach (var expression in Expressions)
            {
                expression.Value.Code = expression.Key;
                expression.Value.Check(this);
            }

            DependenciesCreated = true;
        }

        public void Update(string DecimalSeparator, string TimeZone, InstanceCacheObject cacheObject = null)
        {
            Dictionary<string, object> InstanceRuntimeValues = cacheObject == null ? new Dictionary<string, object>() : new Dictionary<string, object> {
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

            if (!DependenciesCreated)
                CreateDependencies();

            ////first evaluation
            //foreach (var expression in Expressions)
            //    expression.Value.Evaluate(this, TimeZone);

            //Update(DecimalSeparator, TimeZone, InstanceRuntimeValues);

            ////second evaluation, to consider expressions with item properties which depends by expression values, higher level dependencies and cycles are still a problem
            //foreach (var expression in Expressions.Values)
            //    expression.Evaluate(this, TimeZone);

            //Update(DecimalSeparator, TimeZone, InstanceRuntimeValues);
        }
        
        private void Update(string DecimalSeparator, string TimeZone, Dictionary<string, object> InstanceRuntimeValues)
        {
            DescriptionFormula?.Update(this, InstanceRuntimeValues, DecimalSeparator, TimeZone, null);
            FileNameFormula?.Update(this, InstanceRuntimeValues, DecimalSeparator, TimeZone, null);

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
    }
}
