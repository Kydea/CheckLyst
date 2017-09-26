using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Amazon.SQS;
using Autofac;
using CheckListManager.Common.Core.Cache;
using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Cache;

namespace CheckListManager.Common.Logic.Terminal.Configuration
{
	public class TerminalConfig
	{
		private Assembly[] _assemblies;

		private Assembly[] Assemblies
			=> _assemblies ?? (_assemblies = LoadRefencedAssemblies()
					.Where(a => a.FullName.StartsWith($"{nameof(CheckListManager)}."))
					.ToArray());

		public ContainerBuilder CreateBuilder()
		{
			var builder = new ContainerBuilder();

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
				.InstancePerLifetimeScope();

			builder.RegisterAssemblyTypes(Assemblies)
				.AssignableTo<IRepository>()
				.AsImplementedInterfaces()
				.InstancePerLifetimeScope();

			builder.RegisterType<RelationalDatabaseEntities>()
				.AsSelf()
				.InstancePerRequest();

			builder.RegisterAssemblyTypes(Assemblies)
				.AssignableTo<IRedisCache>()
				.AsImplementedInterfaces()
				.InstancePerDependency();

			builder.RegisterType<AmazonSQSConfig>()
				.AsSelf()
				.SingleInstance()
				.OnActivating(e => e.Instance.ServiceURL = "https://sqs.eu-west-1.amazonaws.com/"); //TODO resolve dependency and get from config

			builder.RegisterType<AmazonSQSClient>()
				.As<IAmazonSQS>()
				.AsSelf()
				.InstancePerLifetimeScope();

			return builder;
		}

		private static IEnumerable<Assembly> LoadRefencedAssemblies()
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
