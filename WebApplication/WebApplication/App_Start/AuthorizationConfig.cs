using Microsoft.Owin.Security.OAuth;
using Owin;

namespace CheckListManager.WebApplication
{
    public class AuthorizationConfig
    {
        public static void Setup(IAppBuilder app)
        {
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());
		}
	}
}