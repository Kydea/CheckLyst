using System.Linq;
using System.Reflection;
using System.Web.Compilation;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using Common.Core.Cache;
using Common.Core.Services;
using StackExchange.Redis;

namespace Common.Logic.Configuration
{
	public static class ApiConfig
	{
		public static void Configure(this HttpConfiguration configuration)
		{
			var builder = new ContainerBuilder();
			var assemblies = builder
				.RegisterAssemblyTypes(BuildManager.GetReferencedAssemblies().Cast<Assembly>().ToArray());

			assemblies
				.AssignableTo<IService>()
				.AsImplementedInterfaces()
				.SingleInstance();

			assemblies
				.AssignableTo<ICache>()
				.AsImplementedInterfaces()
				.SingleInstance();

			builder.RegisterInstance<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(""));

			configuration.DependencyResolver = new AutofacWebApiDependencyResolver(builder.Build());
		}
	}
}
