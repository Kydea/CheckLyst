using System.Collections.Generic;
using CheckListManager.Common.Logic.Models.Expressions;
using Newtonsoft.Json;

namespace CheckListManager.Common.Logic.Models.CheckLists
{
	public interface ICheckListField : ICheckListEditableItem
    {
		string ExternalSourceColumn { get; set; }
		string ExternalSourceName { get; set; }
		List<FieldRecord> History { get; }
		int LinesCount { get; set; }
		string Value { get; set; }
		string ValueFormulaString { get; set; }
		List<string> ValuesList { get; set; }
		[JsonIgnore]
		DataType ValueType { get; }
		string ValueTypeName { get; set; }
        string DefaultValue { get; set; }
        string Format { get; set; }
        int? Precision { get; set; }
        bool LocalTimeZone { get; set; }
    }
}