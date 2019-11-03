using System.Collections.Generic;
using System.Linq;
using CheckListManager.Common.Logic.Models;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions;
using Newtonsoft.Json;

namespace CheckListManager.Prototypes.Logic.Models.CheckLists
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

        private List<string> _ValuesList;
        public List<string> ValuesList { get { return _ValuesList; } set { _ValuesList = value == null || !value.Any() ? null : value; } }

        [JsonIgnore]
        public DataType ValueType { get; private set; } = DataType.Text;
        public string ValueTypeName
        {
            get { return ValueType.TypeName; }
            set { ValueType = DataType.FromName(value); }
        }

        public bool LocalTimeZone { get; set; }

        public override ICheckListItem Clone(ICheckListGroup parent)
        {
            var field = new Field
            {
                Group = parent,
                LinesCount = LinesCount,
                Value = Value,
                ValueFormula = ValueFormula == null ? null : new Formula(ValueFormula.RawValue),
                ValueTypeName = ValueTypeName,
                LocalTimeZone = LocalTimeZone,
                ValuesList = ValuesList,
                ExternalSourceName = ExternalSourceName,
                ExternalSourceColumn = ExternalSourceColumn
            };
            Copy(this, field);
            return field;
        }
    }
}
