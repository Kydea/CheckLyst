using System.ComponentModel.DataAnnotations;

namespace CheckListManager.AccessControl.Logic.Messages.Users
{
	public sealed class UserRequest
	{
		public string Code { get; set; }
		public bool Enabled { get; set; }
		public string Password { get; set; }
        public string Password2 { get; set; }
        public string Description { get; set; }
		public string Email { get; set; }
        public string AuthenticationCode { get; set; }
        public Function[] GeneralFunctions { get; set; }
        public Function[] PrototypeFunctions { get; set; }
        public Function[] InstanceFunctions { get; set; }

        public string Reason { get; set; }
    }
}
