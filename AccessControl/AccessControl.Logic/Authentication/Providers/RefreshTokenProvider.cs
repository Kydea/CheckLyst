using System;
using System.Linq;
using System.Threading.Tasks;
using CheckListManager.AccessControl.Logic.Services;
using CheckListManager.Common.Logic.Services;
using Microsoft.Owin;
using Microsoft.Owin.Security.Infrastructure;

namespace CheckListManager.AccessControl.Logic.Authentication.Providers
{
	public class RefreshTokenProvider : IAuthenticationTokenProvider
	{
		private readonly IUserService _userService;
		private readonly IHashService _hashService;

		public RefreshTokenProvider(IUserService userService, IHashService hashService)
		{
			_userService = userService;
			_hashService = hashService;
		}

		public async Task CreateAsync(AuthenticationTokenCreateContext context)
		{
			if (context.OwinContext.Environment.TryGetValue("Microsoft.Owin.Form#collection", out var inputs))
			{
				var grantType = ((FormCollection)inputs)?.GetValues("grant_type");
				var grant = grantType?.FirstOrDefault();

				if (grant == null || grant.Equals("refresh_token")) return;
			}

			var guid = Guid.NewGuid().ToString();
			var company = long.Parse(context.Ticket.Properties.Dictionary["company"]);
			var sessionCode = _hashService.HashStringToString($"{company}-{guid}");
			var expiresOn = DateTimeOffset.UtcNow.AddDays(1); //TODO set right value
			var user = context.Ticket.Properties.Dictionary["code"];

			var ticket = context.SerializeTicket();

			await _userService.CreateSessionAsync(company, sessionCode, user, ticket, expiresOn);
			context.SetToken(sessionCode);
		}

		public async Task ReceiveAsync(AuthenticationTokenReceiveContext context)
		{
			var token = await _userService.GetTokenAsync(context.Token);
			if (token != null)
			{
				context.DeserializeTicket(token.AuthenticationTicket);
			}
		}

		#region Not Implemented

		public void Create(AuthenticationTokenCreateContext context)
		{
		}

		public void Receive(AuthenticationTokenReceiveContext context)
		{
		}

		#endregion
	}
}