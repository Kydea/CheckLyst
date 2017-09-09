using System.Web.Http;
using Autofac;
using CheckListManager.AccessControl.Logic.Providers;
using CheckListManager.Common.Logic.Configuration;
using Microsoft.Owin.Security.OAuth;

namespace CheckListManager.AccessControl
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
	        config.Configure((builder, assemblies) =>
	        {
		        builder
			        .RegisterAssemblyTypes(assemblies)
			        .AssignableTo<AuthTokenProvider>()
			        .AsSelf()
			        .SingleInstance();
		        builder
			        .RegisterAssemblyTypes(assemblies)
			        .AssignableTo<RefreshTokenProvider>()
			        .AsSelf()
			        .SingleInstance();
	        });

            // Web API configuration and services
            // Configure Web API to use only bearer token authentication.
            config.SuppressDefaultHostAuthentication();
	        config.Filters.Add(new AuthorizeAttribute());
			config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

            // Web API routes
            config.MapHttpAttributeRoutes();

            //config.Routes.MapHttpRoute(
            //    "DefaultApi",
            //    "api/{controller}/{id}",
            //    new { id = RouteParameter.Optional }
            //);
        }
    }
}
