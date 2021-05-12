using CheckListManager.Common.Logic.DataEngine;

namespace CheckListManager.AccessControl.Logic.Messages.Users
{
	public class UserResponse : Form
	{
		public string Code { get; set; }
        public bool ShowPassword { get; set; }
        public bool ShowAuthenticationCode { get; set; }
        public Grant[] GeneralFunctions { get; set; }
        public Grant[] PrototypeFunctions { get; set; }
        public Grant[] InstanceFunctions { get; set; }
    }
}
