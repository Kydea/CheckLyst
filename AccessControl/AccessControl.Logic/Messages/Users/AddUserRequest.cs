using System.ComponentModel.DataAnnotations;

namespace CheckListManager.AccessControl.Logic.Messages.Users
{
	public sealed class AddUserRequest
	{
		[Required]
		public string Code { get; set; }
		[Required]
		public bool Enabled { get; set; }
		[Required]
		public string Password { get; set; }
		[Required]
		public string Description { get; set; }

		public string Email { get; set; }
	}
}
