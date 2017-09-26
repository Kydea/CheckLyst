using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Autofac;
using CheckListManager.AccessControl.Api;
using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Data;
using SpecFlow.Autofac;

namespace CheckListManager.AccessControl.Tests.Setup
{
	public static class DependencyInjection
	{
		[ScenarioDependencies]
		public static ContainerBuilder RegisterTypesWithSpecflowContainer()
		{
			var assemblies = LoadRefencedAssemblies();
			var config = new WebApiConfig();
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
