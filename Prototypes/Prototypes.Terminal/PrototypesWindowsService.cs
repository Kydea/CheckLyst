using System.Threading.Tasks;
using Autofac;
using CheckListManager.Common.Logic.Terminal;
using CheckListManager.Common.Logic.Terminal.Configuration;
using CheckListManager.Common.Logic.Terminal.Scheduler;
using CheckListManager.Prototypes.Logic.Services;

namespace CheckListManager.Prototypes.Terminal
{
	public class PrototypesWindowsService : TimerWindowsService
	{
		public PrototypesWindowsService(TerminalConfig config) : base(config)
        {
            Add(new ComputedSchedulerStrategy(UpdatePrototypeVersionsState, new[] { 0 }, new[] { 0 }) { RunAtStartup = true });
            Add(new IntervalSchedulerStrategy(SendFTPFiles, 60) { RunAtStartup = true });
        }

        private async Task UpdatePrototypeVersionsState()
		{
			using (var scope = Container.BeginLifetimeScope())
			{
				var service = scope.Resolve<IPrototypeService>();
                await service.UpdatePrototypeVersionsState(CancellationTokenSource.Token);
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

    }
}
