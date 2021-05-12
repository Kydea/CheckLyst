using CheckListManager.AccessControl.Logic.Messages.Users;
using System.ComponentModel.DataAnnotations;

namespace CheckListManager.AccessControl.Logic.Messages.Roles
{
	public sealed class RoleRequest
	{
		public string Code { get; set; }
		public string FunctionGroup { get; set; }
        public Grant[] GeneralFunctions { get; set; }
        public Grant[] PrototypeFunctions { get; set; }
        public Grant[] InstanceFunctions { get; set; }

        public string Reason { get; set; }
    }
}
