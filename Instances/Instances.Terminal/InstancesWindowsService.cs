using System.Threading.Tasks;
using Autofac;
using CheckListManager.Common.Logic.Terminal;
using CheckListManager.Common.Logic.Terminal.Configuration;
using CheckListManager.Common.Logic.Terminal.Scheduler;
using CheckListManager.Instances.Logic.Services;

namespace CheckListManager.Instances.Terminal
{
	public class InstancesWindowsService : TimerWindowsService
	{
		public InstancesWindowsService(TerminalConfig config) : base(config)
		{
            Add(new IntervalSchedulerStrategy(ClearPendingInstances, 3600));
            Add(new IntervalSchedulerStrategy(SendFTPFiles, 60));
        }

        protected async Task ClearPendingInstances()
		{
			using (var scope = Container.BeginLifetimeScope())
			{
				var service = scope.Resolve<IInstanceManagerService>();
				await service.ClearPendingInstances(CancellationTokenSource.Token);
			}
		}

        protected async Task SendFTPFiles()
        {
            using (var scope = Container.BeginLifetimeScope())
            {
                var service = scope.Resolve<IFTPService>();
                await service.ProcessFTPMessagesAsync(CancellationTokenSource.Token);
            }
        }
    }
}
