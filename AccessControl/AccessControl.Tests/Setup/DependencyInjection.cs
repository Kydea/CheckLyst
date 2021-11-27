using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using Amazon.SQS;
using Autofac;
using CheckListManager.AccessControl.Api;
using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Configuration;
using CheckListManager.Common.Logic.Services;
using RabbitMQ.Client;
using SpecFlow.Autofac;

namespace CheckListManager.AccessControl.Tests.Setup
{
	public static class DependencyInjection
	{
		[ScenarioDependencies]
		public static ContainerBuilder RegisterTypesWithSpecflowContainer()
		{
			var assemblies = LoadRefencedAssemblies();
			var config = new WebApiConfig { LoadAssemblies = false };
			var builder = config.CreateBuilder();

			builder.RegisterType<RelationalDatabaseEntities>()
				.AsSelf()
				.InstancePerLifetimeScope();

			builder.RegisterAssemblyTypes(assemblies)
				.AssignableTo<IRepository>()
				.AsImplementedInterfaces()
				.InstancePerLifetimeScope();

			builder.RegisterAssemblyTypes(assemblies)
				.AssignableTo<IService>()
				.AsImplementedInterfaces()
				.InstancePerLifetimeScope();

			builder
				.RegisterAssemblyTypes(typeof(DependencyInjection).Assembly)
				.InstancePerLifetimeScope();

			var useRabbitMq = "RabbitMQ".Equals(ConfigurationManager.AppSettings["QueueSystem"], StringComparison.InvariantCultureIgnoreCase);
			if (useRabbitMq)
			{
				builder.RegisterType<ConnectionFactory>()
					.As<IConnectionFactory>()
					.AsSelf()
					.SingleInstance()
					.OnActivating(ConfigBase.OnConfiguringRabbitMq);
				
				builder.RegisterType<QueueServiceRabbitMq>()
					.As<IQueueService>()
					.AsSelf()
					.InstancePerLifetimeScope();
			}
			else
			{
				builder.RegisterType<AmazonSQSConfig>()
					.AsSelf()
					.SingleInstance()
					.OnActivating(ConfigBase.OnConfiguringSqs);

				builder.RegisterType<AmazonSQSClient>()
					.As<IAmazonSQS>()
					.AsSelf()
					.InstancePerLifetimeScope();
				
				builder.RegisterType<QueueServiceSqs>()
					.As<IQueueService>()
					.AsSelf()
					.InstancePerLifetimeScope();
			}

			return builder;
		}

		private static Assembly[] LoadRefencedAssemblies()
		{
			var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic).ToList();
			var availableAssemblies = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "CheckListManager*.dll")
				.Select(AssemblyName.GetAssemblyName);
			var toLoad = availableAssemblies.Where(x => loadedAssemblies.All(a => a.FullName != x.FullName)).ToList();
			loadedAssemblies.AddRange(toLoad.Select(name => AppDomain.CurrentDomain.Load(name)));

			return loadedAssemblies.ToArray();
		}
	}
}
