using CheckListManager.AccessControl.Logic.Messages.Users;
using CheckListManager.Common.Logic;
using System;
using System.Collections.Generic;

namespace CheckListManager.AccessControl.Logic.Messages.Roles
{
	public sealed class RolesResponse
	{
		public string Code { get; set; }
		public string FunctionGroup { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedOn { get; set; }

        public List<string> Functions { get; set; }
        public PascalCaseDictionary<string, List<string>> FunctionGroups { get; set; }
    }
}
