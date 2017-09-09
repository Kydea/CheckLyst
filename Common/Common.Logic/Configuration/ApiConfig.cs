using System;
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
		public static void Configure(this HttpConfiguration configuration, Action<ContainerBuilder, Assembly[]> setupCustomDependencies = null)
		{
			var assembliesLookup =
				BuildManager
					.GetReferencedAssemblies()
					.Cast<Assembly>()
					.Where(a => a.FullName.StartsWith($"{nameof(CheckListManager)}."))
					.ToArray();

			var builder = new ContainerBuilder();

			builder.RegisterAssemblyTypes(assembliesLookup)
				.AssignableTo<IRedisSetup>()
				.AsImplementedInterfaces()
				.SingleInstance();

			builder.RegisterAssemblyTypes(assembliesLookup)
				.AssignableTo<IService>()
				.AsImplementedInterfaces()
				.InstancePerDependency();

			builder.RegisterAssemblyTypes(assembliesLookup)
				.AssignableTo<IRepository>()
				.AsImplementedInterfaces()
				.InstancePerDependency();

			builder.RegisterAssemblyTypes(assembliesLookup)
				.AssignableTo<IMemoryCache>()
				.AsImplementedInterfaces()
				.SingleInstance();

			builder.RegisterAssemblyTypes(assembliesLookup)
				.AssignableTo<IRedisCache>()
				.AsImplementedInterfaces()
				.InstancePerDependency();

			builder.RegisterAssemblyTypes(assembliesLookup)
				.AssignableTo<RelationalDatabaseEntities>()
				.AsSelf()
				.InstancePerDependency();

			builder.RegisterApiControllers(assembliesLookup);

			setupCustomDependencies?.Invoke(builder, assembliesLookup);

			GlobalConfiguration.Configuration.DependencyResolver =
				configuration.DependencyResolver
					= new AutofacWebApiDependencyResolver(builder.Build());
		}
	}
}
