using System;
using System.Collections.Generic;
using System.Linq;
using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Logic.Dependency;
using CheckListManager.Common.Logic.Models;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions;
using Newtonsoft.Json;

namespace CheckListManager.Instances.Logic.Models.CheckLists
{
    public class Field : Item, ICheckListField
    {
        public override string TypeName => "Field";

        public string ExternalSourceColumn { get; set; }
        public string ExternalSourceName { get; set; }
        public List<FieldRecord> History { get; } = new List<FieldRecord>();
        public int LinesCount { get; set; }
        public string Value { get; set; }
        public Formula ValueFormula { get; } = new Formula();
        public string ValueFormulaString { get; set; }

        private List<string> _ValuesList;
        public List<string> ValuesList { get { return _ValuesList; } set { _ValuesList = value == null || !value.Any() ? null : value; } }

        [JsonIgnore]
        public DataType ValueType { get; private set; } = DataType.Text;

        public string ValueTypeName
        {
            get { return ValueType.TypeName; }
            set { ValueType = DataType.FromName(value); }
        }

        public string DefaultValue { get; set; }

        public bool LocalTimeZone { get; set; }

        public int Offset { get; private set; }

        public override ICheckListItem Clone(ICheckListGroup parent)
        {
            var field = new Field
            {
                Group = parent,
                LinesCount = LinesCount,
                Value = Value,
                ValueFormulaString = ValueFormulaString,
                ValueTypeName = ValueTypeName,
                DefaultValue = DefaultValue,
                LocalTimeZone = LocalTimeZone,
                ValuesList = ValuesList,
                ExternalSourceName = ExternalSourceName,
                ExternalSourceColumn = ExternalSourceColumn
            };
            field.ValueFormula.BuildDependencies(ValueFormulaString);
            Copy(this, field);
            return field;
        }

        public override void BuildDependencies()
        {
            base.BuildDependencies();

            DependencyNodeKey DependencyNodeKey = this.DependencyNodeKey.Clone(DependencyNodeKey.Processes.Value, "value");
            ValueFormula.BuildDependencies(ValueFormulaString);
            foreach (var Key in ValueFormula.ValueKeys)
                if (CheckList.Expressions.TryGetValue(Key, out var Expression))
                {
                    Expression.DependentNodes.Add(DependencyNodeKey);
                    DependingNodes.Add(Expression.DependencyNodeKey);
                }
        }

        protected override void UpdateValue()
        {
            base.UpdateValue();

            DateTimeOffset? dateTimeOffset = null;
            if (ValueType == DataType.Date)
                dateTimeOffset = DateTimeOffsetHelper.Parse(Value, Instance.TimeZone, out var Ambiguous, out var AmbiguousDateTimeOffsets, DateTimeOffsetHelper.DateFormat);
            else if (ValueType == DataType.DateTime)
                dateTimeOffset = DateTimeOffsetHelper.Parse(Value, Instance.TimeZone, out var Ambiguous, out var AmbiguousDateTimeOffsets, $"{DateTimeOffsetHelper.DateFormat} {DateTimeOffsetHelper.TimeFormat}");
            if (dateTimeOffset.HasValue) Offset = (int)dateTimeOffset.Value.Offset.TotalMinutes;

            Dictionary<string, object> RuntimeValues = new Dictionary<string, object>();
            if (Iteration != null) RuntimeValues["IterationIndex"] = Iteration.Index;
            ValueFormula.Evaluate(CheckList, RuntimeValues, Instance.DecimalSeparator, Instance.TimeZone, Iteration?.Indexes);
            if (ValueFormula.IsSet)
            {
                ConversionError = ValueFormula.HasValue && !ValueType.Validate(ValueFormula.Value);
                Value = ConversionError ? null : ValueFormula.Value;
            }

            if (ExternalSourceName != null)
            {
                string Value =
                    ExternalSourceName != null && Instance.ExternalSourceRows.ContainsKey(ExternalSourceName) && Instance.ExternalSourceRows[ExternalSourceName].ContainsKey(ExternalSourceColumn) ?
                    Instance.ExternalSourceRows[ExternalSourceName][ExternalSourceColumn] :
                    null;
                ConversionError = !ValueType.Validate(Value);
                this.Value = ConversionError ? null : Value;
            }
        }

        public bool Validate(string value) => ValueType.Validate(value);

        public bool ConversionError { get; set; }

        public override string ValidityMessage { get => ConversionError ? $"the value {ValueFormula.Value} could not be converted to {ValueType.TypeName}" : base.ValidityMessage; set => base.ValidityMessage = value; }
        public override bool IsValidByDefault() => !ConversionError && (ValueFormula.IsSet ? ValueFormula.HasValue : !string.IsNullOrEmpty(Value));
        protected override bool IsReadOnlyByDefault() => base.IsReadOnlyByDefault() || ValueFormula.IsSet; // || !string.IsNullOrEmpty(ExternalSourceName);

        protected override bool OnTraverseDependencyChain(ICheckList CheckList, Stack<string> DependencyChain, DependencyNodeKey.Processes Process, Func<IDependencyNode, bool> DependencyChainAction)
        {
            var Result = true;
            if (
                Process == DependencyNodeKey.Processes.Validity 
                && (ValidCondition == Conditions.Default || ValidCondition == Conditions.ExpressionOrDefault || ValidCondition == Conditions.ExpressionAndDefault)
                && !TraverseDependencyChain(CheckList, DependencyChain, DependencyNodeKey.Processes.Value, DependencyChainAction)
            )
                Result = false;

            if (!base.OnTraverseDependencyChain(CheckList, DependencyChain, Process, DependencyChainAction))
                Result = false;

            return Result;
        }

        protected override bool OnMarkNodeAsChanged(ICheckList CheckList, List<IDependencyNode> Roots, DependencyNodeKey.Processes Process)
        {
            var Marked = false;

            if (
                Process == DependencyNodeKey.Processes.Value
                && (ValidCondition == Conditions.Default || ValidCondition == Conditions.ExpressionOrDefault || ValidCondition == Conditions.ExpressionAndDefault)
            )
            {
                MarkNodeAsChanged(CheckList, Roots, DependencyNodeKey.Processes.Validity);
                Marked = true;
            }

            if (base.OnMarkNodeAsChanged(CheckList, Roots, Process))
                Marked = true;

            return Marked;
        }

        public override void Setup()
        {
            base.Setup();
            if (!string.IsNullOrEmpty(DefaultValue))
            {
                if (Validate(DefaultValue)) Value = DefaultValue;
                DefaultValue = null;
            }
        }
    }
}
