using TechTalk.SpecFlow;

namespace CheckListManager.Instances.Tests.Hooks
{
	[Binding]
	public sealed class InstanceCacheHooks
	{
		[AfterScenario("CleanupRedis")]
		public static void CleanupRedis()
		{
			//var context = ScenarioContext.Current.ScenarioContainer.Resolve<IComponentContext>();
			//var service = context.Resolve<IInstanceCacheService>();

			//var testContext = context.Resolve<InstanceCacheContext>();

			//service.RemoveAsync(testContext.Code, testContext.UserCode);
		}
	}
}
