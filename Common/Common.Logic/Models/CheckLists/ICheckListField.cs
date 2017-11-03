using System.Collections.Generic;
using CheckListManager.Common.Logic.Models.Expressions;
using Newtonsoft.Json;

namespace CheckListManager.Common.Logic.Models.CheckLists
{
	public interface ICheckListField : ICheckListItem
	{
		string ExternalSourceColumn { get; set; }
		string ExternalSourceName { get; set; }
		List<FieldRecord> History { get; }
		int LinesCount { get; set; }
		RawValue Value { get; set; }
		[JsonIgnore]
		Formula ValueFormula { get; }
		string ValueFormulaString { get; set; }
		List<string> ValuesList { get; set; }
		[JsonIgnore]
		DataType ValueType { get; }
		string ValueTypeName { get; set; }
	}
}