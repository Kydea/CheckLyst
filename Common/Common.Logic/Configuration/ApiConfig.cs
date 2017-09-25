using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web.Compilation;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using Autofac;
using Autofac.Integration.WebApi;
using AutoMapper;
using CheckListManager.Common.Core.Cache;
using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Authentication;
using CheckListManager.Common.Logic.Cache;
using CheckListManager.Common.Logic.Filters;
using Microsoft.Owin.Security.OAuth;
using NLog;

namespace CheckListManager.Common.Logic.Configuration
{
	public abstract class ApiConfig
	{
		public bool LoadAssemblies { get; set; }

		private Assembly[] _assemblies;

		public Assembly[] Assemblies
			=> _assemblies ?? (_assemblies = (LoadAssemblies
								? BuildManager
									.GetReferencedAssemblies()
									.Cast<Assembly>()
								: AppDomain.CurrentDomain.GetAssemblies())
							.Where(a => a.FullName.StartsWith($"{nameof(CheckListManager)}."))
							.ToArray());


		public ContainerBuilder CreateBuilder()
		{
			var builder = new ContainerBuilder();

			builder.RegisterApiControllers(Assemblies);

			builder.RegisterAssemblyTypes(Assemblies)
				.AssignableTo<IRedisSetup>()
				.AsImplementedInterfaces()
				.SingleInstance();

			builder.RegisterAssemblyTypes(Assemblies)
				.AssignableTo<ISingletonService>()
				.AsImplementedInterfaces()
				.SingleInstance();

			builder.RegisterAssemblyTypes(Assemblies)
				.AssignableTo<IMemoryCache>()
				.AsImplementedInterfaces()
				.SingleInstance();

			builder.RegisterAssemblyTypes(Assemblies)
				.AssignableTo<IService>()
				.AsImplementedInterfaces()
				.InstancePerRequest();

			builder.RegisterAssemblyTypes(Assemblies)
				.AssignableTo<IRepository>()
				.AsImplementedInterfaces()
				.InstancePerRequest();

			builder.RegisterType<RelationalDatabaseEntities>()
				.AsSelf()
				.InstancePerRequest();

			builder.RegisterAssemblyTypes(Assemblies)
				.AssignableTo<IRedisCache>()
				.AsImplementedInterfaces()
				.InstancePerDependency();

			builder.RegisterInstance(CreateMapper())
				.SingleInstance();

			OnCreateBuilder(builder);

			return builder;
		}

		protected virtual void OnCreateBuilder(ContainerBuilder builder)
		{
			
		}

		public IMapper CreateMapper()
		{
			return new Mapper(new MapperConfiguration(OnCreateMapper));
		}

		protected virtual void OnCreateMapper(IMapperConfigurationExpression mapperConfig)
		{
			
		}
	}

	public static class ApiConfigExtensions
	{
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

		public static void Configure(this HttpConfiguration configuration, ApiConfig apiConfig)
		{
			ConfigureLog();
			ConfigureDependencyInjection(configuration, apiConfig.CreateBuilder());

			configuration.Services.Replace(typeof(IExceptionHandler), new DefaultExceptionHandler());

			configuration.SuppressDefaultHostAuthentication();
			configuration.Filters.Add(new AuthorizeAttribute());
			configuration.Filters.Add(new DefaultAuthenticationFilter(OAuthDefaults.AuthenticationType));
			configuration.Filters.Add(new ArgumentNullValidationActionFilterAttribute());
			configuration.Filters.Add(new ModelValidationActionFilterAttribute());

			configuration.MapHttpAttributeRoutes();
		}

		private static void ConfigureLog()
		{
			try
			{
				if (!EventLog.SourceExists(nameof(CheckListManager), "Application"))
				{
					EventLog.CreateEventSource(nameof(CheckListManager), "Application");
				}
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
	}
}
