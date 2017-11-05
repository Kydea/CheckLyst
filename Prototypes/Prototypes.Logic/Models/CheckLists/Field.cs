using System.Collections.Generic;
using CheckListManager.Common.Logic.Models;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions;

namespace CheckListManager.Prototypes.Logic.Models.CheckLists
{
	public class Field : Item, ICheckListField
	{
		public override string TypeName => "Field";

		public string ExternalSourceColumn { get; set; }
		public string ExternalSourceName { get; set; }
		public List<FieldRecord> History { get; } = new List<FieldRecord>();
		public int LinesCount { get; set; }
		public RawValue Value { get; set; }

		public Formula ValueFormula { get; private set; }

		public string ValueFormulaString
		{
			get => ValueFormula?.RawValue;
			set => ValueFormula = value == null ? null : new Formula(value);
		}

		public List<string> ValuesList { get; set; }
		public DataType ValueType { get; } = DataType.Text;
		public string ValueTypeName { get; set; }
	}
}
