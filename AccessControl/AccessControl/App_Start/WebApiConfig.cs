using System.Reflection;
using System.Web.Http;
using Autofac;
using CheckListManager.AccessControl.Logic.Authentication.Providers;
using CheckListManager.Common.Logic.Configuration;

namespace CheckListManager.AccessControl
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
	        var builder = CreateBuilder();
	        config.Configure(builder);
        }

		public static ContainerBuilder CreateBuilder(bool loadAssemblies = true)
	    {
		    var builder = ApiConfig.CreateBuilder(out Assembly[] assemblies, loadAssemblies);
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