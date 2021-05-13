namespace CheckListManager.AccessControl.Logic.Messages.Users
{
	public sealed class UsersRequest
	{
        public long? CompanyId { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string User { get; set; }
        public string CorporateID { get; set; }
        public string Group { get; set; }
    }
}
