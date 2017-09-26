using System;
using System.Web.Http;
using Autofac;
using Autofac.Core;
using Autofac.Core.Lifetime;
using CheckListManager.AccessControl.Logic.Authentication.Providers;
using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using Owin;

namespace CheckListManager.AccessControl.Api
{
	public partial class Startup
	{
		private static OAuthAuthorizationServerOptions OAuthOptions { get; set; }

		private static void ConfigureAuth(IAppBuilder app)
		{
			var parentScope =
				(ILifetimeScope)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(ILifetimeScope));
			using (var scope = parentScope.BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag, builder =>
			{
				builder.RegisterType<AuthTokenProvider>().SingleInstance();
				builder.RegisterType<RefreshTokenProvider>().SingleInstance();
				builder.Register(c => (ISharingLifetimeScope)c.Resolve<ILifetimeScope>())
					.As<ISharingLifetimeScope>()
					.ExternallyOwned();
			}))
			{
				OAuthOptions = new OAuthAuthorizationServerOptions
				{
					TokenEndpointPath = new PathString("/Access/Token"),
					Provider = scope.Resolve<AuthTokenProvider>(),
					AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(30), //TODO load from configuration
					RefreshTokenProvider = scope.Resolve<RefreshTokenProvider>(),
					AllowInsecureHttp = true					
				};

				app.UseOAuthBearerTokens(OAuthOptions);
			}
		}
	}
}
