using System;

namespace CheckListManager.AccessControl.Logic.Authentication
{
	public class IdentityToken
	{
		public IdentityToken()
		{
			
		}

		public IdentityToken(string authenticationTicket, string refreshToken, DateTimeOffset expiresOn)
		{
			AuthenticationTicket = authenticationTicket;
			RefreshToken = refreshToken;
			ExpiresOn = expiresOn;
		}

		public string AuthenticationTicket { get; set; }
		public string RefreshToken { get; set; }
		public DateTimeOffset ExpiresOn { get; set; }
	}
}
