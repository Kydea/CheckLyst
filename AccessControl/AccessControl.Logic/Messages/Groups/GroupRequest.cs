using CheckListManager.AccessControl.Logic.Messages.Users;
using System.ComponentModel.DataAnnotations;

namespace CheckListManager.AccessControl.Logic.Messages.Groups
{
	public sealed class GroupRequest
    {
		public string Code { get; set; }
        public string Description { get; set; }
        public Function[] GeneralFunctions { get; set; }
        public Function[] PrototypeFunctions { get; set; }
        public Function[] InstanceFunctions { get; set; }

        public string Reason { get; set; }
    }
}
