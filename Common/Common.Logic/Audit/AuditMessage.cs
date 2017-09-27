using System;

namespace CheckListManager.Common.Logic.Audit
{
	public class AuditMessage
	{
		public long Company { get; set; }
		public string User { get; set; }
		public DateTimeOffset Date { get; set; }
		public string Action { get; set; }
		public string Information { get; set; }
		public long? Prototype { get; set; }
		public int? Version { get; set; }
		public long? Instance { get; set; }
	}
}
