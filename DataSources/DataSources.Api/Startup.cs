using CheckListManager.DataSources.Api;
using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using Owin;

[assembly: OwinStartup(typeof(Startup))]

namespace CheckListManager.DataSources.Api
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
	        app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());
        }
    }
}
