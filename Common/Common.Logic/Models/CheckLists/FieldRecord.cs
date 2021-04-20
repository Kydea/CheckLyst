using System;

namespace CheckListManager.Common.Logic.Models.CheckLists
{
	public struct FieldRecord
	{
		public DateTimeOffset DateTime { get; set; }
		public string User { get; set; }
		public string Value { get; set; }
        public string TypedValue { get; set; }
	}
}