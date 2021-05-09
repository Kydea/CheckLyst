using System;

namespace CheckListManager.AccessControl.Logic.Messages.Roles
{
	public sealed class RolesResponse
	{
		public string Code { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedOn { get; set; }
    }
}
