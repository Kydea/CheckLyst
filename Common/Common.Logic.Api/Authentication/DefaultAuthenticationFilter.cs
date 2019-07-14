using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Filters;
using CheckListManager.Common.Core.Web.Security;

namespace CheckListManager.Common.Logic.Api.Authentication
{
    public class DefaultAuthenticationFilter : IAuthenticationFilter
    {
        private readonly HostAuthenticationFilter _baseAuthentication;

        public DefaultAuthenticationFilter(string authenticationType)
        {
            _baseAuthentication = new HostAuthenticationFilter(authenticationType);
        }

        public bool AllowMultiple { get; } = false;

        public async Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            await _baseAuthentication.AuthenticateAsync(context, cancellationToken);

            if (context.Principal?.Identity != null && context.Principal.Identity.IsAuthenticated)
                context.Principal =
                    new ClaimsPrincipal(new DefaultIdentity((ClaimsIdentity)context.Principal.Identity));
        }

        public async Task ChallengeAsync(HttpAuthenticationChallengeContext context,
            CancellationToken cancellationToken)
        {
            await _baseAuthentication.ChallengeAsync(context, cancellationToken);
        }
    }
}