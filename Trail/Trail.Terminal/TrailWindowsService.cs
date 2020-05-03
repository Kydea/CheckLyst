using System.Threading.Tasks;
using Autofac;
using CheckListManager.Trail.Logic.Services;
using CheckListManager.Common.Logic.Terminal;
using CheckListManager.Common.Logic.Terminal.Configuration;
using CheckListManager.Common.Logic.Terminal.Scheduler;

namespace CheckListManager.Trail.Terminal
{
	public class TrailWindowsService : TimerWindowsService
	{
		public TrailWindowsService(TerminalConfig config) : base(config)
		{
            Add(new IntervalSchedulerStrategy(RunAsync, 5));
		}

		private async Task RunAsync()
		{
			using (var scope = Container.BeginLifetimeScope())
			{
				var service = scope.Resolve<ITrailService>();
				await service.ProcessMessagesAsync(CancellationTokenSource.Token);
			}
		}
	}
}
