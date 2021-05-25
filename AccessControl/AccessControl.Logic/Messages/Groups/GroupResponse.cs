using CheckListManager.AccessControl.Logic.Messages.Users;
using CheckListManager.Common.Logic.DataEngine;

namespace CheckListManager.AccessControl.Logic.Messages.Groups
{
	public class GroupResponse : Form
	{
		public string Code { get; set; }
        public string Description { get; set; }
        public Grant[] GeneralFunctions { get; set; }
        public Grant[] PrototypeFunctions { get; set; }
        public Grant[] InstanceFunctions { get; set; }
    }
}
