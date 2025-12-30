using System;

namespace CheckListManager.AccessControl.Logic.Messages.Users
{
	public sealed class UsersResponse
	{
		public string Code { get; set; }
        public string Description { get; set; }
		public string Email { get; set; }
		public string CorporateID { get; set; }
        public DateTimeOffset? LastLoginOn { get; set; }
        public int NumberOfPasswordAttempts { get; set; }
        public bool Enabled { get; set; }
        public string AuthenticationCode { get; set; }
        public string ReportWorkFlowLevel { get; set; }
        public string TimeZone { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedOn { get; set; }
    }
}
