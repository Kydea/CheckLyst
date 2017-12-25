using System.Threading.Tasks;
using Autofac;
using CheckListManager.Common.Logic.Terminal;
using CheckListManager.Common.Logic.Terminal.Configuration;
using CheckListManager.Common.Logic.Terminal.Scheduler;
using CheckListManager.Infrastructure.Logic.Email;

namespace CheckListManager.Infrastructure.Terminal
{
	public class InfrastructureWindowsService : TimerWindowsService
	{
		public InfrastructureWindowsService(TerminalConfig config) : base(config, new IntervalSchedulerStrategy(10))
		{
		}

		protected override async Task RunAsync()
		{
			using (var scope = Container.BeginLifetimeScope())
			{
				var service = scope.Resolve<IEmailService>();
				await service.ProcessEmailMessagesAsync(CancellationTokenSource.Token);
			}
		}
	}
}
