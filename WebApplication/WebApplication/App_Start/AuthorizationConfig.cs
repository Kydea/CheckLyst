using Microsoft.Owin.Security.OAuth;
using Owin;

namespace CheckListManager.WebApplication
{
    public class AuthorizationConfig
    {
        public static void Setup(IAppBuilder App)
        {
			App.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());
		}
	}
}