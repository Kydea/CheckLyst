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
            Add(new IntervalSchedulerStrategy(ClearPendingInstances, 3600) { RunAtStartup = true });
            Add(new IntervalSchedulerStrategy(SendFTPFiles, 60) { RunAtStartup = true });
            Add(new ComputedSchedulerStrategy(SendNotifications, new[] { 1 }));
        }

        private async Task ClearPendingInstances()
		{
			using (var scope = Container.BeginLifetimeScope())
			{
				var service = scope.Resolve<IInstanceManagerService>();
				await service.ClosePendingInstances(CancellationTokenSource.Token);
			}
		}

		private async Task SendFTPFiles()
        {
            using (var scope = Container.BeginLifetimeScope())
            {
                var service = scope.Resolve<IFTPService>();
                await service.ProcessFTPMessagesAsync(CancellationTokenSource.Token);
            }
        }

        private async Task SendNotifications()
        {
            using (var scope = Container.BeginLifetimeScope())
            {
                var service = scope.Resolve<IInstanceManagerService>();
                await service.SendNotificationsAsync(CancellationTokenSource.Token);
            }
        }
    }
}
