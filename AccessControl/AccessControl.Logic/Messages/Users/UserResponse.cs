using CheckListManager.Common.Logic.DataEngine;

namespace CheckListManager.AccessControl.Logic.Messages.Users
{
	public class UserResponse : Form
	{
		public string Code { get; set; }
        public bool ShowPassword { get; set; }
        public bool ShowAuthenticationCode { get; set; }
        public Function[] GeneralFunctions { get; set; }
        public Function[] PrototypeFunctions { get; set; }
        public Function[] InstanceFunctions { get; set; }
    }
}
