using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Amazon.SQS;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using AutoMapper;
using CheckListManager.Common.Core.Cache;
using CheckListManager.Common.Core.Network;
using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Cache;
using CheckListManager.Common.Logic.Services;
using RedLock;

namespace CheckListManager.Common.Logic.Configuration
{
	public abstract class ConfigBase
	{
		protected virtual bool UseInstancePerRequest { get; } = false;

		private Assembly[] _assemblies;

		protected Assembly[] Assemblies
			=> _assemblies ?? (_assemblies = GetAssemblies()
					.Where(a => a.FullName.StartsWith($"{nameof(CheckListManager)}."))
					.ToArray());

		protected virtual Assembly[] GetAssemblies()
		{
			var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic).ToList();
			var availableAssemblies = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "CheckListManager*.dll")
				.Select(AssemblyName.GetAssemblyName);
			var toLoad = availableAssemblies.Where(x => loadedAssemblies.All(a => a.FullName != x.FullName)).ToList();
			loadedAssemblies.AddRange(toLoad.Select(name => AppDomain.CurrentDomain.Load(name)));

			return loadedAssemblies.ToArray();
		}

		public ContainerBuilder CreateBuilder()
		{
			var builder = new ContainerBuilder();
			OnCreateBuilder(builder);
			return builder;
		}

		protected virtual void OnCreateBuilder(ContainerBuilder builder)
		{
			builder.RegisterType<RedisSetup>()
				.AsSelf()
				.As<IRedisSetup>()
				.SingleInstance();

			builder.RegisterType<RedisLockFactory>()
				.As<IRedisLockFactory>()
				.SingleInstance()
				.UsingConstructor(typeof(RedisLockEndPoint[]));

			builder.RegisterAssemblyTypes(Assemblies)
				.AssignableTo<ISingletonService>()
				.AsImplementedInterfaces()
				.SingleInstance();

			builder.RegisterAssemblyTypes(Assemblies)
				.AssignableTo<IMemoryCache>()
				.AsImplementedInterfaces()
				.SingleInstance();

			builder.RegisterInstance(CreateMapper())
				.SingleInstance();

			builder.RegisterType<RedisLockEndPoint>()
				.AsSelf()
				.SingleInstance()
				.OnActivating(OnConfiguringRedis);

			builder.RegisterType<AmazonSQSConfig>()
				.AsSelf()
				.SingleInstance()
				.OnActivating(OnConfiguringSqs);

			builder.RegisterAssemblyTypes(Assemblies)
				.AssignableTo<IService>()
				.AsImplementedInterfaces()
				.InstancePerScope(UseInstancePerRequest);

			builder.RegisterAssemblyTypes(Assemblies)
				.AssignableTo<IRepository>()
				.AsImplementedInterfaces()
				.InstancePerScope(UseInstancePerRequest);

			builder.RegisterType<RelationalDatabaseEntities>()
				.AsSelf()
				.InstancePerScope(UseInstancePerRequest);

			builder.RegisterAssemblyTypes(Assemblies)
				.AssignableTo<IRedisCache>()
				.AsImplementedInterfaces()
				.InstancePerDependency();

			builder.RegisterType<AmazonSQSClient>()
				.As<IAmazonSQS>()
				.AsSelf()
				.InstancePerScope(UseInstancePerRequest);

			builder.Register(ConfigureServiceClient)
				.As<IServiceClient>()
				.As<ServiceClient>()
				.InstancePerDependency();
		}

		private static void OnConfiguringRedis(IActivatingEventArgs<RedisLockEndPoint> e)
		{
			var configuration = e.Context.Resolve<IConfigurationService>();
			e.Instance.EndPoint = new DnsEndPoint(configuration.RedisHost, configuration.RedisPort);
		}

		private static void OnConfiguringSqs(IActivatingEventArgs<AmazonSQSConfig> e)
		{
			var configuration = e.Context.Resolve<IConfigurationService>();
			e.Instance.ServiceURL = configuration.SqsBaseUrl;
		}

		private static ServiceClient ConfigureServiceClient(IComponentContext context, IEnumerable<Parameter> parameters)
		{
			string baseUrl = null;
			try
			{
				baseUrl = parameters.Named<string>("baseUrl");
			}
			catch (Exception)
			{
				// ignored
			}

			if (baseUrl == null)
			{
				var configuration = context.Resolve<IConfigurationService>();
				baseUrl = configuration.ApiBaseUrl;
			}
			return new ServiceClient(baseUrl);
		}

		private IMapper CreateMapper()
		{
			return new Mapper(new MapperConfiguration(OnCreateMapper));
		}

		protected virtual void OnCreateMapper(IMapperConfigurationExpression mapperConfig)
		{

		}
	}

	internal static class ConfigExtensions
	{
		public static IRegistrationBuilder<TLimit, TActivatorData, TStyle> InstancePerScope<TLimit, TActivatorData, TStyle>(
			this IRegistrationBuilder<TLimit, TActivatorData, TStyle> registration, bool useInstancePerRequest)
		{
			return useInstancePerRequest
				? registration.InstancePerRequest()
				: registration.InstancePerLifetimeScope();
		}
	}
}
