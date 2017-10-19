using System.Threading.Tasks;
using Autofac;
using CheckListManager.Trail.Logic.Services;
using CheckListManager.Common.Logic.Terminal;
using CheckListManager.Common.Logic.Terminal.Configuration;

namespace CheckListManager.Trail.Terminal
{
	public class TrailWindowsService : TimerWindowsService
	{
		public TrailWindowsService(TerminalConfig config) : base(config, 5000)
		{
		}

		protected override bool StopTimerWhenRunning => true;

		protected override async Task RunAsync()
		{
			using (var scope = Container.BeginLifetimeScope())
			{
				var service = scope.Resolve<ITrailService>();
				await service.ProcessMessagesAsync(CancellationTokenSource.Token);
			}
		}
	}
}
