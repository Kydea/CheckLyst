using CheckListManager.AccessControl.Logic.Exceptions;

namespace CheckListManager.AccessControl.Tests.Contexts
{
	public class LoginContext
	{
		public string Username { get; set; }
		public string Password { get; set; }

		public AuthenticationException Exception { get; set; }
	}
}
