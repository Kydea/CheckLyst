using System.Threading.Tasks;
using Autofac;
using CheckListManager.Common.Logic.Terminal;
using CheckListManager.Common.Logic.Terminal.Configuration;
using CheckListManager.Common.Logic.Terminal.Scheduler;

namespace CheckListManager.Infrastructure.Terminal
{
	public class InfrastructureWindowsService : TimerWindowsService
	{
		public InfrastructureWindowsService(TerminalConfig config) : base(config)
		{
            Add(new IntervalSchedulerStrategy(RunAsync, 10));
		}

		protected async Task RunAsync()
		{
			using (var scope = Container.BeginLifetimeScope())
			{
                await Task.CompletedTask;
				//var emailService = scope.Resolve<IEmailService>();
				//await emailService.ProcessEmailMessagesAsync(CancellationTokenSource.Token);
            }
        }
	}
}
