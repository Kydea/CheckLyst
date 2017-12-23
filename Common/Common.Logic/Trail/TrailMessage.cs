using System;
using System.Collections.Generic;

namespace CheckListManager.Common.Logic.Trail
{
	public class TrailMessage
	{
		public long Company { get; set; }
        public long Id { get; set; }
		public string User { get; set; }
		public DateTimeOffset Date { get; set; }
		public string Action { get; set; }
		public string Information { get; set; }
		public string ShortInformation { get; set; }
		public string CheckList { get; set; }
    }
}
