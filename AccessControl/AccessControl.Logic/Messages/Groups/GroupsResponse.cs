using System;
using System.Collections.Generic;

namespace CheckListManager.AccessControl.Logic.Messages.Groups
{
	public sealed class GroupsResponse
    {
		public string Code { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedOn { get; set; }
        public List<string> Users { get; internal set; }
    }
}
