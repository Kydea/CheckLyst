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
		public PrototypesWindowsService(TerminalConfig config) : base(config, new ComputedSchedulerStrategy(new[] { 0 }, new[] { 0 }) { RunAtStartup = true })
        {
        }

        protected override async Task RunAsync()
		{
			using (var scope = Container.BeginLifetimeScope())
			{
				var service = scope.Resolve<IPrototypeService>();
                await service.UpdatePrototypeVersionsState(CancellationTokenSource.Token);
			}
		}
	}
}
