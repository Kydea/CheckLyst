using Autofac;
using SpecFlow.Autofac;
using TechTalk.SpecFlow;

namespace CheckListManager.AccessControl.Tests.Setup
{
	public static class DependencyInjection
	{
		[ScenarioDependencies]
		public static ContainerBuilder RegisterTypesWithSpecflowContainer(ScenarioContext context)
		{
			var builder = WebApiConfig.CreateBuilder();

			return builder;
		}
	}
}
