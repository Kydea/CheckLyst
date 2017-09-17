using System;
using System.Web.Http;
using CheckListManager.AccessControl.Logic.Authentication.Providers;
using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using Owin;

namespace CheckListManager.AccessControl
{
	public partial class Startup
	{
		private static OAuthAuthorizationServerOptions OAuthOptions { get; set; }

		// ReSharper disable once MemberCanBeMadeStatic.Global
		// ReSharper disable once MemberCanBePrivate.Global
		public void ConfigureAuth(IAppBuilder app)
		{
			OAuthOptions = new OAuthAuthorizationServerOptions
			{
				TokenEndpointPath = new PathString("/Access/Token"),
				Provider = (AuthTokenProvider)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(AuthTokenProvider)),
				AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(30),
				RefreshTokenProvider = (RefreshTokenProvider)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(RefreshTokenProvider)),
				AllowInsecureHttp = true
			};

			app.UseOAuthBearerTokens(OAuthOptions);
		}
	}
}
