using CheckListManager.AccessControl.Logic.Messages.Users;
using CheckListManager.Common.Logic.DataEngine;

namespace CheckListManager.AccessControl.Logic.Messages.Roles
{
	public class RoleResponse : Form
	{
		public string Code { get; set; }
		public string FunctionGroup { get; set; }
        public Grant[] GeneralFunctions { get; set; }
        public Grant[] PrototypeFunctions { get; set; }
        public Grant[] InstanceFunctions { get; set; }
    }
}
