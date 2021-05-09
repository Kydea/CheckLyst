namespace CheckListManager.AccessControl.Logic.Messages.Roles
{
	public sealed class RolesRequest
    {
        public long? CompanyId { get; set; }
        public string Code { get; set; }
    }
}
