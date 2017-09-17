using System.ComponentModel.DataAnnotations;

namespace CheckListManager.AccessControl.Logic.Messages.Logout
{
	public class LogoutRequest
	{
		[Required]
		public string RefreshToken { get; set; }
	}
}
