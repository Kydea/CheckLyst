using System.ComponentModel.DataAnnotations;

namespace CheckListManager.AccessControl.Logic.Messages.Access
{
	public sealed class AuthenticateRequest
    {
		public string Code { get; set; }
		public string Password { get; set; }
	}
}
