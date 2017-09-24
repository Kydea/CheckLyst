using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Filters;

namespace CheckListManager.Common.Logic.Authentication
{
	public class AuthenticationFilter : IAuthenticationFilter
	{
		private readonly HostAuthenticationFilter _baseAuthentication;

		public AuthenticationFilter(string authenticationType)
		{
			_baseAuthentication = new HostAuthenticationFilter(authenticationType);
		}

		public bool AllowMultiple { get; } = false;

		public async Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
		{
			await _baseAuthentication.AuthenticateAsync(context, cancellationToken);

			if (context.Principal != null)
			{
				context.Principal = new ClaimsPrincipal(new Identity((ClaimsIdentity)context.Principal.Identity));
			}
		}

		public async Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
		{
			await _baseAuthentication.ChallengeAsync(context, cancellationToken);
		}
	}
}
