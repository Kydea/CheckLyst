using System.ComponentModel.DataAnnotations;

namespace CheckListManager.AccessControl.Logic.Messages.Access
{
	public sealed class LogoutRequest
	{
		[Required]
		public string RefreshToken { get; set; }
	}
}
