using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;

namespace CheckListManager.AccessControl.Logic.Providers
{
	public class RefreshTokenProvider : IAuthenticationTokenProvider
	{
		private static readonly ConcurrentDictionary<string, AuthenticationTicket> RefreshTokens = new ConcurrentDictionary<string, AuthenticationTicket>();

		public void Create(AuthenticationTokenCreateContext context)
		{
		}

		public async Task CreateAsync(AuthenticationTokenCreateContext context)
		{
			if (context.OwinContext.Environment.TryGetValue("Microsoft.Owin.Form#collection", out object inputs))
			{
				var grantType = ((FormCollection)inputs)?.GetValues("grant_type");
				var grant = grantType?.FirstOrDefault();

				if (grant == null || grant.Equals("refresh_token")) return;
			}

			var guid = Guid.NewGuid().ToString();

			// copy all properties and set the desired lifetime of refresh token  
			var refreshTokenProperties = new AuthenticationProperties(context.Ticket.Properties.Dictionary)
			{
				IssuedUtc = context.Ticket.Properties.IssuedUtc,
				ExpiresUtc = DateTime.UtcNow.AddMinutes(60)//DateTime.UtcNow.AddYears(1)  
			};
			var refreshTokenTicket = new AuthenticationTicket(context.Ticket.Identity, refreshTokenProperties);

			RefreshTokens.TryAdd(guid, refreshTokenTicket);

			// consider storing only the hash of the handle  
			context.SetToken(guid);

			await Task.FromResult(true);
		}

		public void Receive(AuthenticationTokenReceiveContext context)
		{
			throw new NotImplementedException();
		}

		public async Task ReceiveAsync(AuthenticationTokenReceiveContext context)
		{
			if (RefreshTokens.TryGetValue(context.Token, out AuthenticationTicket ticket))
			{
				context.SetTicket(ticket);
			}

			await Task.FromResult(true);
		}
	}
}