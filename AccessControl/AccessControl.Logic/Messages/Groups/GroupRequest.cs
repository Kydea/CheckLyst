using CheckListManager.AccessControl.Logic.Messages.Users;
using System.ComponentModel.DataAnnotations;

namespace CheckListManager.AccessControl.Logic.Messages.Groups
{
	public sealed class GroupRequest
    {
		public string Code { get; set; }
        public string Description { get; set; }
        public Grant[] GeneralFunctions { get; set; }
        public Grant[] PrototypeFunctions { get; set; }
        public Grant[] InstanceFunctions { get; set; }

        public string Reason { get; set; }
    }
}
