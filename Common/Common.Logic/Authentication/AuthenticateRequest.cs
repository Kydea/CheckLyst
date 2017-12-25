namespace CheckListManager.Common.Logic.Authentication
{
    public sealed class AuthenticateRequest
    {
		public string Code { get; set; }
		public string Password { get; set; }
	}
}
