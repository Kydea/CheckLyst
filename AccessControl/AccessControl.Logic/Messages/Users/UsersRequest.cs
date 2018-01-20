namespace CheckListManager.AccessControl.Logic.Messages.Users
{
	public sealed class UsersRequest
	{
		public string Code { get; set; }
        public string Description { get; set; }
        public string User { get; set; }
        public long? CompanyId { get; set; }
    }
}
