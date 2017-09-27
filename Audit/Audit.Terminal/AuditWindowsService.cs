using System.Threading.Tasks;
using Autofac;
using CheckListManager.Audit.Logic.Services;
using CheckListManager.Common.Logic.Terminal;
using CheckListManager.Common.Logic.Terminal.Configuration;

namespace CheckListManager.Audit.Terminal
{
	public class AuditWindowsService : TimerWindowsService
	{
		public AuditWindowsService(TerminalConfig config) : base(config, 60000)
		{
		}

		protected override bool StopTimerWhenRunning => true;

		protected override async Task RunAsync()
		{
			using (var scope = Container.BeginLifetimeScope())
			{
				var service = scope.Resolve<IAuditService>();
				await service.ProcessMessagesAsync(CancellationTokenSource.Token);
			}
		}
	}
}
