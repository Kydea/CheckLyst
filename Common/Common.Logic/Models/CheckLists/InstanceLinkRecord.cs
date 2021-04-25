using System;
using System.Collections.Generic;

namespace CheckListManager.Common.Logic.Models.CheckLists
{
	public struct InstanceLinkRecord
    {
		public long? ID { get; set; }
        public string Code { get; set; }
        public string State { get; set; }
        public string Description { get; set; }
        public DateTimeOffset? DateTime { get; set; }
        public string User { get; set; }
        public PascalCaseDictionary<string, string> Tags { get; set; }
	}
}