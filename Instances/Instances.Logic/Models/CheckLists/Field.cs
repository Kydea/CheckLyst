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
        public string ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedOn { get; set; }
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
        public int? Precision { get; set; }

        public bool LocalTimeZone { get; set; }

        [JsonProperty]
        public int Offset { get; private set; }

        public override dynamic GetViewModel()
        {
            var ItemModel = base.GetViewModel();
            ItemModel.ValueFormulaString = ValueFormulaString;
            ItemModel.ValueTypeName = ValueTypeName;
            ItemModel.Value = Value;
            ItemModel.LinesCount = LinesCount;

            if (ValuesList != null)
                ItemModel.ValuesList = ValuesList;

            if (ExternalSourceName != null)
            {
                ItemModel.ExternalSourceName = ExternalSourceName;
                ItemModel.ExternalSourceColumn = ExternalSourceColumn;
            }

            if (LocalTimeZone)
            {
                ItemModel.LocalTimeZone = LocalTimeZone;
                ItemModel.Offset = Offset;
            }

            if (History.Any())
                ItemModel.History = History.Select(x => new {
                    x.DateTime,
                    x.User,
                    x.Value
                });
            return ItemModel;
        }

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
                Precision = Precision,
                LocalTimeZone = LocalTimeZone,
                ValuesList = ValuesList,
                ExternalSourceName = ExternalSourceName,
                ExternalSourceColumn = ExternalSourceColumn
            };
            field.ValueFormula.IsSet = ValueFormula.IsSet;
            Copy(this, field);
            return field;
        }

        public override void BuildDependencies()
        {
            base.BuildDependencies();

            ValueFormula.BuildDependencies(ValueFormulaString, IterationItem ? ((Field)FindIteratorModelItem()).ValueFormula : null);

            if (!IterationItem)
            {
                foreach (var Key in ValueFormula.ValueKeys)
                    if (CheckList.Expressions.TryGetValue(Key, out var Expression))
                        AddDependingRelation(DependencyRelation.Properties.Value, Expression, DependencyRelation.Properties.Value);

                if (ValidCondition == Conditions.Default || ValidCondition == Conditions.ExpressionOrDefault || ValidCondition == Conditions.ExpressionAndDefault)
                    AddDependingRelation(DependencyRelation.Properties.Validity, this, DependencyRelation.Properties.Value);

                if (ExternalSourceName != null)
                {
                    if (!CheckList.ExternalSources.TryGetValue(ExternalSourceName, out var ExternalSourceDependencyNode))
                        throw new Exception($"The property value references to the inexistent external source {ExternalSourceName} for {this}");

                    AddDependingRelation(DependencyRelation.Properties.Value, ExternalSourceDependencyNode, DependencyRelation.Properties.Value);
                    if (IteratorModelItem)
                    {
                        var Parent = Group;
                        while (!Parent.Iterator)
                            Parent = Parent.Group;
                        ((Group)Parent).ExternalSources.Add(ExternalSourceName);
                    }
                }
            }
        }

        private void UpdateValue()
        {
            if (IterationItem)
                ValueFormula.Evaluate(CheckList, GetRuntimeValues(), Instance.DecimalSeparator, Instance.TimeZone, Iteration?.Indexes, ((Field)FindIteratorModelItem()).ValueFormula);
            else
                ValueFormula.Evaluate(CheckList, GetRuntimeValues(), Instance.DecimalSeparator, Instance.TimeZone, Iteration?.Indexes);

            object ConvertedValue = null;
            if (ValueFormula.IsSet)
            {
                var FormulaValue = ValueFormula.Value;
                ConversionError = ValueFormula.HasValue && !ValueType.TryParse(FormulaValue, out ConvertedValue);
                if (Precision.HasValue && !ConversionError && ConvertedValue is decimal decimalValue)
                    FormulaValue = Math.Round(decimalValue, Precision.Value).ToString();

                Value = ConversionError ? null : FormulaValue;
            }

            if (ExternalSourceName != null && Instance.ExternalSourceDependencyNodes.TryGetValue(ExternalSourceName, out var ExternalSourceDependencyNode) && ExternalSourceDependencyNode.Values != null && ExternalSourceDependencyNode.Values.TryGetValue(ExternalSourceColumn, out var ExternalSourceValue))
            {
                ConversionError = !ValueType.TryParse(ExternalSourceValue, out ConvertedValue);
                if (Precision.HasValue && !ConversionError && ConvertedValue is decimal decimalValue)
                    ExternalSourceValue = Math.Round(decimalValue, Precision.Value).ToString();

                Value = ConversionError ? null : ExternalSourceValue;
            }

            if (!ConversionError)
                try
                {
                    DateTimeOffset? dateTimeOffset = null;
                    if (ValueType == DataType.Date)
                        dateTimeOffset = DateTimeOffsetHelper.Parse(Value, Instance.TimeZone, out var Ambiguous, out var AmbiguousDateTimeOffsets, DateTimeOffsetHelper.DateFormat);
                    else if (ValueType == DataType.DateTime)
                        dateTimeOffset = DateTimeOffsetHelper.Parse(Value, Instance.TimeZone, out var Ambiguous, out var AmbiguousDateTimeOffsets, DateTimeOffsetHelper.DateTimeFormats);
                    if (dateTimeOffset.HasValue) Offset = (int)dateTimeOffset.Value.Offset.TotalMinutes;
                }
                catch(FormatException)
                {
                    ConversionError = true;
                    Value = null;
                }
        }

        protected override void OnUpdate(DependencyRelation.Properties Property)
        {
            switch (Property)
            {
                case DependencyRelation.Properties.Value:
                    UpdateValue();
                    break;
                default:
                    base.OnUpdate(Property);
                    break;
            }
        }

        public override void MarkAllAsChanged(HashSet<Tuple<DependencyNode, DependencyRelation.Properties>> Roots)
        {
            base.MarkAllAsChanged(Roots);
            MarkNodeAsChanged(CheckList, Roots, DependencyRelation.Properties.Value);
        }

        public bool ConversionError { get; set; }

        public override string ValidityMessage { get => ConversionError ? $"the value {ValueFormula.Value} could not be converted to {ValueType.TypeName}" : base.ValidityMessage; set => base.ValidityMessage = value; }
        public override bool IsValidByDefault() => !ConversionError && (ValueFormula.IsSet ? ValueFormula.HasValue : !string.IsNullOrEmpty(Value));
        protected override bool IsReadOnlyByDefault() => base.IsReadOnlyByDefault() || ValueFormula.IsSet; // || !string.IsNullOrEmpty(ExternalSourceName);

        public override bool Setup()
        {
            var Result = base.Setup();
            if (!string.IsNullOrEmpty(DefaultValue))
            {
                if (ValueType.TryParse(DefaultValue, out var Object))
                {
                    Value = DefaultValue;
                    if (Precision.HasValue && Object is decimal DecimalValue)
                        Value = Math.Round(DecimalValue, Precision.Value).ToString();
                }
                DefaultValue = null;
            }
            return Result;
        }
    }
}
