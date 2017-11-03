namespace CheckListManager.Common.Logic.Models.CheckLists
{
	public struct SignatureRecord
	{
		public string DateTime { get; set; }

		public string User { get; set; }

		public string Signature { get; set; }

		public string Note { get; set; }

		public string Action => "<center><i class=\"fa fa-fw fa-" + (Note == null ? "check" : "remove") + "\"></i></center>";
	}
}
