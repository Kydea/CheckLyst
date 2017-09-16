using System;
using System.Transactions;
using TechTalk.SpecFlow;

namespace CheckListManager.AccessControl.Tests.Hooks
{
	[Binding]
	public sealed class DatabaseHooks
	{
		[BeforeScenario("WithTransaction")]
		public static void CreateTransaction()
		{
			var transaction = new TransactionScope(TransactionScopeOption.RequiresNew, TimeSpan.FromMinutes(10),
				TransactionScopeAsyncFlowOption.Enabled);
			ScenarioContext.Current.Set(transaction);			
		}

		[AfterScenario("WithTransaction")]
		public static void RollbackTransaction()
		{
			var transaction = ScenarioContext.Current.Get<TransactionScope>();
			transaction?.Dispose();
		}
	}
}
