using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using Owin;
using System;

namespace CheckListManager.WebApplication
{
    public class AuthorizationConfig
    {
        private static OAuthAuthorizationServerOptions OAuthOptions { get; set; }

        public static void Setup(IAppBuilder App)
        {
            OAuthOptions = new OAuthAuthorizationServerOptions
            {
                AllowInsecureHttp = true
            };

            App.UseOAuthBearerTokens(OAuthOptions);
        }
    }
}