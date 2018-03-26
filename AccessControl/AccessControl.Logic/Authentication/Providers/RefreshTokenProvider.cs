using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using Autofac.Core.Lifetime;
using CheckListManager.AccessControl.Logic.Services;
using CheckListManager.Common.Logic.Services;
using Microsoft.Owin;
using Microsoft.Owin.Security.Infrastructure;

namespace CheckListManager.AccessControl.Logic.Authentication.Providers
{
	public class RefreshTokenProvider : IAuthenticationTokenProvider
	{
		private const int RefreshTokenTimeSpan = 1; //1 day

		private readonly ILifetimeScope _parentScope;
		private readonly IHashService _hashService;

		public RefreshTokenProvider(ISharingLifetimeScope parentScope, IHashService hashService)
		{
			_parentScope = parentScope.RootLifetimeScope;
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
			var expiresOn = DateTimeOffset.UtcNow.AddDays(RefreshTokenTimeSpan);
			var user = context.Ticket.Properties.Dictionary["code"];

			var ticket = context.SerializeTicket();

			using (var scope = _parentScope.BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag))
			{
				var userService = scope.Resolve<IUserService>();
				await userService.CreateSessionAsync(company, sessionCode, user, ticket, expiresOn);
				context.SetToken(sessionCode);
			}
		}

		public async Task ReceiveAsync(AuthenticationTokenReceiveContext context)
		{
			using (var scope = _parentScope.BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag))
			{
				var userService = scope.Resolve<IUserService>();
				var token = await userService.GetTokenAsync(context.Token);
				if (token != null)
				{
					await userService.UpdateTokenAsync(token, DateTimeOffset.UtcNow.AddDays(RefreshTokenTimeSpan));
					context.DeserializeTicket(token.AuthenticationTicket);

					context.Ticket.Properties.ExpiresUtc = token.ExpiresOn;
				}
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