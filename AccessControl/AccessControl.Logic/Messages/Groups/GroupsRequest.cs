namespace CheckListManager.AccessControl.Logic.Messages.Groups
{
	public sealed class GroupsRequest
    {
        public long? CompanyId { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
    }
}
