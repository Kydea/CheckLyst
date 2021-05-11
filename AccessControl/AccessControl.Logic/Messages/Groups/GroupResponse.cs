using CheckListManager.AccessControl.Logic.Messages.Users;
using CheckListManager.Common.Logic.DataEngine;

namespace CheckListManager.AccessControl.Logic.Messages.Groups
{
	public class GroupResponse : Form
	{
		public string Code { get; set; }
        public string Description { get; set; }
        public Function[] GeneralFunctions { get; set; }
        public Function[] PrototypeFunctions { get; set; }
        public Function[] InstanceFunctions { get; set; }
    }
}
