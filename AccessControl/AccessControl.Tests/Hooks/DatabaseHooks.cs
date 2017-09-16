using Autofac;
using CheckListManager.Common.Data;
using TechTalk.SpecFlow;

namespace CheckListManager.AccessControl.Tests.Hooks
{
	[Binding]
	public sealed class DatabaseHooks
	{
		[BeforeScenario("WithTransaction")]
		public static void CreateTransaction()
		{
			var context = ScenarioContext.Current.ScenarioContainer.Resolve<IComponentContext>();
			var dc = context.Resolve<RelationalDatabaseEntities>();
			dc.Database.BeginTransaction();
		}

		[AfterScenario("WithTransaction")]
		public static void RollbackTransaction()
		{
			var context = ScenarioContext.Current.ScenarioContainer.Resolve<IComponentContext>();
			var dc = context.Resolve<RelationalDatabaseEntities>();

			dc.Database.CurrentTransaction?.Dispose();
		}
	}
}
