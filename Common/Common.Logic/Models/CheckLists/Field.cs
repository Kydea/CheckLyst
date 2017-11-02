using System.Collections.Generic;
using CheckListManager.Common.Logic.Models.Expressions;
using Newtonsoft.Json;

namespace CheckListManager.Common.Logic.Models.CheckLists
{
    public class Field : Item
    {
        public override string TypeName => "Field";

	    public int LinesCount { get; set; }

        [JsonIgnore]
        public DataType ValueType { get; private set; } = DataType.Text;

        public string ValueTypeName
        {
            get => ValueType.TypeName;
	        set => ValueType = DataType.FromName(value);
        }

        [JsonIgnore]
        public Formula ValueFormula { get; private set; }

        public string ValueFormulaString
        {
            get => ValueFormula?.RawValue;
	        set => ValueFormula = value == null ? null : new Formula(value);
        }

        public List<string> ValuesList { get; set; }

        public string ExternalSourceName { get; set; }

        public string ExternalSourceColumn { get; set; }

        private string ExternalSourceValue =>
			CheckList is Instance instance
			&& ExternalSourceName != null
			&& instance.ExternalSourceRows.ContainsKey(ExternalSourceName)
			&& instance.ExternalSourceRows[ExternalSourceName].ContainsKey(ExternalSourceColumn)
				? instance.ExternalSourceRows[ExternalSourceName][ExternalSourceColumn]
				: null;

	    public RawValue Value { get; set; }

        public string GetValue()
        {
            return Value?.Value ?? ValueFormula?.Value;
        }

        public override bool IsValidByDefault()
        {
            return ValueFormula?.HasValue ?? !string.IsNullOrEmpty(Value);
        }

        protected override bool IsReadOnlyByDefault()
        {
            return base.IsReadOnlyByDefault() || ValueFormula != null;
        }

        public override Item Clone(Group parent)
        {
            var field = new Field()
            {
                Group = parent,
                LinesCount = LinesCount,
                Value = Value,
                ValueFormula = ValueFormula == null ? null : new Formula(ValueFormula.RawValue),
                ValueTypeName = ValueTypeName,
                ValuesList = ValuesList,
                ExternalSourceName = ExternalSourceName,
                ExternalSourceColumn = ExternalSourceColumn,
            };
            Copy(this, field);
            return field;
        }

        public override void Update()
        {
            base.Update();
	        ValueFormula?.Update(CheckList.Expressions, RuntimeValues);
        }

        public void UpdateValues(uint clientId)
        {
            if (!IsReadOnly(clientId) && ExternalSourceName != null) Value = ExternalSourceValue;
        }

        public bool Validate(string value)
		{
			return ValueType.Validate(value);
		}

		public override dynamic GetModel(uint clientId)
        {
            var result = base.GetModel(clientId);
            result.LinesCount = LinesCount;
            result.ValueTypeName = ValueTypeName;
            result.Value = GetValue();
            result.ValueFormula = ValueFormula?.RawValue;
            result.ValuesList = ValuesList;
            result.ExternalSourceName = ExternalSourceName;
            result.ExternalSourceColumn = ExternalSourceColumn;
            result.History = History;
            return result;
        }

		public List<Record> History { get; } = new List<Record>();

        public struct Record
        {
            public string DateTime { get; set; }

            public string User { get; set; }

            public string Value { get; set; }
        }
    }
}
