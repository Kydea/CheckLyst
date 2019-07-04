using System;

namespace CheckListManager.Common.Logic.Models.CheckLists
{
	public struct SignatureRecord
	{
		public DateTimeOffset DateTime { get; set; }

		public string User { get; set; }

		public string Signature { get; set; }

		public string Note { get; set; }

		public string Action => "<center><i class=\"fal fa-" + (Note == null ? "check" : "trash") + "\"></i></center>";
	}
}
