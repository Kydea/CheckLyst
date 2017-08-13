using System.Linq;
using System.Reflection;
using System.Web.Compilation;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using CheckListManager.Common.Core.Cache;
using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Cache;
using StackExchange.Redis;

namespace CheckListManager.Common.Logic.Configuration
{
	public static class ApiConfig
	{
		public static void Configure(this HttpConfiguration configuration)
		{
			var assembliesLookup =
				BuildManager
					.GetReferencedAssemblies()
					.Cast<Assembly>()
					.Where(a => a.FullName.StartsWith($"{nameof(CheckListManager)}."))
					.ToArray();

			var builder = new ContainerBuilder();

			var registration = builder.RegisterAssemblyTypes(assembliesLookup);

			builder
				.RegisterInstance<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(""))
				.SingleInstance()
				.ExternallyOwned();

			registration
				.AssignableTo<IService>()
				.AsImplementedInterfaces()
				.InstancePerDependency();

			registration
				.AssignableTo<IRepository>()
				.AsImplementedInterfaces()
				.InstancePerDependency();

			registration
				.AssignableTo<IMemoryCache>()
				.AsImplementedInterfaces()
				.SingleInstance();

			registration
				.AssignableTo<IRedisCache>()
				.AsImplementedInterfaces()
				.InstancePerDependency();

			builder.RegisterApiControllers(assembliesLookup);

			configuration.DependencyResolver = new AutofacWebApiDependencyResolver(builder.Build());
		}
	}
}
