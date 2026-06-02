using CheckListManager.Common.Logic.DataEngine;
using System;

namespace CheckListManager.AccessControl.Logic.Messages.Users
{
	public sealed class UsersRequest: Request
	{
        public long? CompanyId { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string User { get; set; }
        public string CorporateID { get; set; }
        public string Group { get; set; }
        public string Role { get; set; }
        public bool LockedOut { get; set; }
        public int? FailedLogins { get; set; }
        public int? InactivityDays { get; set; }
        public string Enabled { get; set; }
        public DateTimeOffset? From { get; set; }
    }
}
