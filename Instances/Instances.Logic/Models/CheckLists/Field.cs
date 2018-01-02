using System.Collections.Generic;
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

        [JsonIgnore]
        public Formula ValueFormula { get; private set; }

		public string ValueFormulaString
		{
			get => ValueFormula?.RawValue;
			set => ValueFormula = value == null ? null : new Formula(value);
		}

		public List<string> ValuesList { get; set; }

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
		}

        public override bool IsValidByDefault()
        {
            return ValueFormula?.HasValue ?? !string.IsNullOrEmpty(Value);
        }

        protected override bool IsReadOnlyByDefault()
		{
			return base.IsReadOnlyByDefault() || ValueFormula != null;
		}

		public bool Validate(string value)
		{
			return ValueType.Validate(value);
		}
	}
}
