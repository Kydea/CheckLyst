using System;
using System.Linq;
using System.Threading.Tasks;
using CheckListManager.AccessControl.Data;
using CheckListManager.Common.Logic.Services;
using Microsoft.Owin;
using Microsoft.Owin.Security.Infrastructure;

namespace CheckListManager.AccessControl.Logic.Providers
{
	public class RefreshTokenProvider : IAuthenticationTokenProvider
	{
		private readonly IUserSessionRepository _userSessionRepository;
		private readonly IHashService _hashService;

		public RefreshTokenProvider(IUserSessionRepository userSessionRepository, IHashService hashService)
		{
			_userSessionRepository = userSessionRepository;
			_hashService = hashService;
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
			var company = long.Parse(context.Ticket.Properties.Dictionary["company"]);
			var sessionCode = _hashService.HashStringToString($"{company}-{guid}");
			var expiresOn = DateTimeOffset.UtcNow.AddDays(1); //TODO set right value
			var user = context.Ticket.Properties.Dictionary["code"];

			var ticket = context.SerializeTicket();

			await _userSessionRepository.CreateAsync(company, sessionCode, user, ticket, expiresOn);
			context.SetToken(sessionCode);
		}

		public async Task ReceiveAsync(AuthenticationTokenReceiveContext context)
		{
			var session = await _userSessionRepository.GetAsync(context.Token);
			if (session != null && DateTimeOffset.UtcNow <= session.ExpiresOn)
			{
				context.DeserializeTicket(session.Ticket);
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