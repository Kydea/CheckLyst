using System;
using System.Collections.Generic;
using System.Linq;
using CheckListManager.Common.Core.Helpers;
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
            var DependencyNodeKey = this.DependencyNodeKey.Clone("value");
            ValueFormula.BuildDependencies(ValueFormulaString);
            foreach (var Key in ValueFormula.ValueKeys)
                if (CheckList.Expressions.TryGetValue(Key, out var Expression))
                {
                    Expression.DependentNodes.Add(DependencyNodeKey);
                    DependingNodes.Add(Expression.DependencyNodeKey);
                }
        }

        protected override void OnUpdate(string DecimalSeparator, string TimeZone)
        {
            base.OnUpdate(DecimalSeparator, TimeZone);

            DateTimeOffset? dateTimeOffset = null;
            if (ValueType == DataType.Date)
                dateTimeOffset = DateTimeOffsetHelper.Parse(Value, TimeZone, out var Ambiguous, out var AmbiguousDateTimeOffsets, DateTimeOffsetHelper.DateFormat);
            else if (ValueType == DataType.DateTime)
                dateTimeOffset = DateTimeOffsetHelper.Parse(Value, TimeZone, out var Ambiguous, out var AmbiguousDateTimeOffsets, $"{DateTimeOffsetHelper.DateFormat} {DateTimeOffsetHelper.TimeFormat}");
            if (dateTimeOffset.HasValue) Offset = (int)dateTimeOffset.Value.Offset.TotalMinutes;

            ValueFormula.Evaluate(CheckList, RuntimeValues, DecimalSeparator, TimeZone, Iteration?.Indexes);
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
        public override bool IsValidByDefault() => ValueFormula.IsSet ? ValueFormula.HasValue : (!ConversionError && !string.IsNullOrEmpty(Value));
        protected override bool IsReadOnlyByDefault() => base.IsReadOnlyByDefault() || ValueFormula.IsSet; // || !string.IsNullOrEmpty(ExternalSourceName);

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
