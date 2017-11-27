using System.ComponentModel.DataAnnotations;

namespace CheckListManager.AccessControl.Logic.Messages.Users
{
	public sealed class UserRequest
	{
		public string Code { get; set; }
		public bool Enabled { get; set; }
		public string Password { get; set; }
		public string Description { get; set; }
		public string Email { get; set; }
	}
}
