using CheckListManager.AccessControl.Logic.Messages.Users;
using CheckListManager.Common.Logic.DataEngine;

namespace CheckListManager.AccessControl.Logic.Messages.Roles
{
	public class RoleResponse : Form
	{
		public string Code { get; set; }
		public string FunctionGroup { get; set; }
        public Function[] GeneralFunctions { get; set; }
        public Function[] PrototypeFunctions { get; set; }
        public Function[] InstanceFunctions { get; set; }
    }
}
