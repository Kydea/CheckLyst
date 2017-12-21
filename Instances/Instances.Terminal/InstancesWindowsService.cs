using System.Threading.Tasks;
using Autofac;
using CheckListManager.Common.Logic.Terminal;
using CheckListManager.Common.Logic.Terminal.Configuration;
using CheckListManager.Instances.Logic.Services;

namespace CheckListManager.Instances.Terminal
{
	public class InstancesWindowsService : TimerWindowsService
	{
		public InstancesWindowsService(TerminalConfig config) : base(config, 3600000)
		{
		}

		protected override bool StopTimerWhenRunning => true;

		protected override async Task RunAsync()
		{
			using (var scope = Container.BeginLifetimeScope())
			{
				var service = scope.Resolve<IInstanceManagerService>();
				await service.ClearPendingInstances(CancellationTokenSource.Token);
			}
		}
	}
}
