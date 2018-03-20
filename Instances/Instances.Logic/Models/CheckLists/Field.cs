using System.Collections.Generic;
using System.Linq;
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

        public string RawValue => Value ?? (ConversionError ? null : ValueFormula?.Value);

        [JsonIgnore]
        public Formula ValueFormula { get; private set; }

        public string ValueFormulaString
        {
            get => ValueFormula?.RawValue;
            set => ValueFormula = string.IsNullOrEmpty(value) ? null : new Formula(value);
        }

        private List<string> _ValuesList;
        public List<string> ValuesList { get { return _ValuesList; } set { _ValuesList = value == null || !value.Any() ? null : value; } }

        [JsonIgnore]
        public DataType ValueType { get; private set; } = DataType.Text;
        public string ValueTypeName
        {
            get { return ValueType.TypeName; }
            set { ValueType = DataType.FromName(value); }
        }

        public override ICheckListItem Clone(ICheckListGroup parent)
        {
            var field = new Field
            {
                Group = parent,
                LinesCount = LinesCount,
                Value = Value,
                ValueFormula = ValueFormula == null ? null : new Formula(ValueFormula.RawValue),
                ValueTypeName = ValueTypeName,
                ValuesList = ValuesList,
                ExternalSourceName = ExternalSourceName,
                ExternalSourceColumn = ExternalSourceColumn
            };
            Copy(this, field);
            return field;
        }

        public override void Update()
        {
            base.Update();
            ValueFormula?.Update(CheckList.Expressions, RuntimeValues);
            if (ValueFormula != null && ValueFormula.HasValue) ConversionError = !ValueType.Validate(ValueFormula.Value);
        }

        public bool ConversionError { get; set; }

        public override bool Valid => !ConversionError && base.Valid;
        public override string ValidityMessage { get => ConversionError ? $"the value {ValueFormula.Value} could not be converted to {ValueType.TypeName}" : base.ValidityMessage; set => base.ValidityMessage = value; }

        public override bool IsValidByDefault()
        {
            return ValueFormula?.HasValue ?? !string.IsNullOrEmpty(Value);
        }

        protected override bool IsReadOnlyByDefault()
        {
            return base.IsReadOnlyByDefault() || ValueFormula != null || !string.IsNullOrEmpty(ExternalSourceName);
        }

        public bool Validate(string value)
        {
            return ValueType.Validate(value);
        }
    }
}
