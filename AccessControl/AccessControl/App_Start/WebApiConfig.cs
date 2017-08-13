using System.Web.Http;
using CheckListManager.Common.Logic.Configuration;
using Microsoft.Owin.Security.OAuth;

namespace CheckListManager.AccessControl
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
	        config.Configure();

            // Web API configuration and services
            // Configure Web API to use only bearer token authentication.
            config.SuppressDefaultHostAuthentication();
            config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
