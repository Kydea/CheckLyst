using System.Threading;
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

        private async Task ClearPendingInstances(CancellationToken cancellationToken)
		{
			using (var scope = Container.BeginLifetimeScope())
			{
				var service = scope.Resolve<IInstanceManagerService>();
				await service.ClosePendingInstances(cancellationToken);
			}
		}

		private async Task SendFTPFiles(CancellationToken cancellationToken)
        {
            using (var scope = Container.BeginLifetimeScope())
            {
                var service = scope.Resolve<IFTPService>();
                await service.ProcessFTPMessagesAsync(cancellationToken);
            }
        }

        private async Task SendNotifications(CancellationToken cancellationToken)
        {
            using (var scope = Container.BeginLifetimeScope())
            {
                var service = scope.Resolve<IInstanceManagerService>();
                await service.SendNotificationsAsync(cancellationToken);
            }
        }
    }
}
