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

			registration
				.AssignableTo<IRedisSetup>()
				.AsImplementedInterfaces()
				.SingleInstance();

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
				.As<RedisCache>()
				.InstancePerDependency();

			builder.RegisterApiControllers(assembliesLookup);

			GlobalConfiguration.Configuration.DependencyResolver =
				configuration.DependencyResolver
					= new AutofacWebApiDependencyResolver(builder.Build());
		}
	}
}
