using System.Reflection;
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
	        var builder = CreateBuilder();
	        config.Configure(builder);

	        config.SuppressDefaultHostAuthentication();
	        config.Filters.Add(new AuthorizeAttribute());
	        config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

	        config.MapHttpAttributeRoutes();
        }

		public static ContainerBuilder CreateBuilder()
	    {
		    var builder = ApiConfig.CreateBuilder(out Assembly[] assemblies);
		    builder.RegisterAssemblyTypes(assemblies)
			    .AssignableTo<AuthTokenProvider>()
			    .AsSelf()
			    .SingleInstance();
		    builder.RegisterAssemblyTypes(assemblies)
			    .AssignableTo<RefreshTokenProvider>()
			    .AsSelf()
			    .SingleInstance();

			return builder;
	    }
	}
}
