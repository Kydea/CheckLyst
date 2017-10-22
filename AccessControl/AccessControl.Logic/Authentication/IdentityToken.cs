using System;

namespace CheckListManager.AccessControl.Logic.Authentication
{
	public class IdentityToken
	{
		public IdentityToken(string authenticationTicket, string refreshToken, DateTimeOffset expiresOn)
		{
			AuthenticationTicket = authenticationTicket;
			RefreshToken = refreshToken;
			ExpiresOn = expiresOn;
		}

		public string AuthenticationTicket { get; }
		public string RefreshToken { get; }
		public DateTimeOffset ExpiresOn { get; }
	}
}