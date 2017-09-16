using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Autofac;
using SpecFlow.Autofac;

namespace CheckListManager.AccessControl.Tests.Setup
{
	public static class DependencyInjection
	{
		[ScenarioDependencies]
		public static ContainerBuilder RegisterTypesWithSpecflowContainer()
		{
			LoadRefencedAssemblies();
			var builder = WebApiConfig.CreateBuilder(false);

			builder
				.RegisterAssemblyTypes(typeof(DependencyInjection).Assembly)
				.SingleInstance();

			return builder;
		}

		private static void LoadRefencedAssemblies()
		{
			var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic).ToList();
			var availableAssemblies = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "CheckListManager*.dll")
				.Select(AssemblyName.GetAssemblyName);
			var toLoad = availableAssemblies.Where(x => loadedAssemblies.All(a => a.FullName != x.FullName)).ToList();
			loadedAssemblies.AddRange(toLoad.Select(name => AppDomain.CurrentDomain.Load(name)));
		}
	}
}
