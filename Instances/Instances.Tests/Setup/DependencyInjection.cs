using Autofac;
using CheckListManager.Instances.Api;
using SpecFlow.Autofac;

namespace CheckListManager.Instances.Tests.Setup
{
	public static class DependencyInjection
	{
		[ScenarioDependencies]
		public static ContainerBuilder RegisterTypesWithSpecflowContainer()
		{
			var config = new WebApiConfig { LoadAssemblies = false, UseInstancePerRequest = false };
			var builder = config.CreateBuilder();

			builder
				.RegisterAssemblyTypes(typeof(DependencyInjection).Assembly)
				.InstancePerLifetimeScope();

			return builder;
		}
	}
}
