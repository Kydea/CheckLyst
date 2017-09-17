using System;
using System.Diagnostics;
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
using Microsoft.Owin.Security.OAuth;
using NLog;

namespace CheckListManager.Common.Logic.Configuration
{
	public static class ApiConfig
	{
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

		public static void Configure(this HttpConfiguration configuration, ContainerBuilder builder)
		{
			ConfigureLog();
			ConfigureDependencyInjection(configuration, builder);

			configuration.SuppressDefaultHostAuthentication();
			configuration.Filters.Add(new AuthorizeAttribute());
			configuration.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

			configuration.MapHttpAttributeRoutes();
		}

		private static void ConfigureLog()
		{
			try
			{
				EventLog.CreateEventSource(nameof(CheckListManager), "Application");
			}
			catch (Exception e)
			{
				Logger.Fatal(e, "Impossible to setup log source.");
			}
		}

		private static void ConfigureDependencyInjection(HttpConfiguration configuration, ContainerBuilder builder)
		{
			GlobalConfiguration.Configuration.DependencyResolver =
				configuration.DependencyResolver
					= new AutofacWebApiDependencyResolver(builder.Build());
		}

		public static ContainerBuilder CreateBuilder(out Assembly[] assembliesLookup, bool loadAssemblies)
		{
			assembliesLookup =
				loadAssemblies
					? BuildManager
						.GetReferencedAssemblies()
						.Cast<Assembly>()
						.Where(a => a.FullName.StartsWith($"{nameof(CheckListManager)}."))
						.ToArray()
					: AppDomain.CurrentDomain.GetAssemblies();

			var builder = new ContainerBuilder();

			builder.RegisterApiControllers(assembliesLookup);

			builder.RegisterAssemblyTypes(assembliesLookup)
				.AssignableTo<IRedisSetup>()
				.AsImplementedInterfaces()
				.SingleInstance();

			builder.RegisterAssemblyTypes(assembliesLookup)
				.AssignableTo<ISingletonService>()
				.AsImplementedInterfaces()
				.SingleInstance();

			builder.RegisterAssemblyTypes(assembliesLookup)
				.AssignableTo<IMemoryCache>()
				.AsImplementedInterfaces()
				.SingleInstance();

			builder.RegisterAssemblyTypes(assembliesLookup)
				.AssignableTo<IService>()
				.AsImplementedInterfaces()
				.InstancePerRequest();

			builder.RegisterAssemblyTypes(assembliesLookup)
				.AssignableTo<IRepository>()
				.AsImplementedInterfaces()
				.InstancePerRequest();

			builder.RegisterType<RelationalDatabaseEntities>()
				.AsSelf()
				.InstancePerRequest();

			builder.RegisterAssemblyTypes(assembliesLookup)
				.AssignableTo<IRedisCache>()
				.AsImplementedInterfaces()
				.InstancePerDependency();

			return builder;
		}
	}
}
