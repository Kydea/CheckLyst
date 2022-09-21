using System.Configuration;
using System.Threading.Tasks;
using Autofac;
using CheckListManager.Common.Logic.Terminal;
using CheckListManager.Common.Logic.Terminal.Configuration;
using CheckListManager.Common.Logic.Terminal.Scheduler;
using CheckListManager.MigrationTools.Terminal.InstancesAnalysis;

namespace CheckListManager.MigrationTools.Terminal
{
	public class MigrationToolsWindowsService : TimerWindowsService
	{
		public MigrationToolsWindowsService(TerminalConfig config) : base(config)
        {
            Add(new IntervalSchedulerStrategy(AnalyzeAsync, int.Parse(ConfigurationManager.AppSettings["InstancesAnalysisInterval"])) { RunAtStartup = true });
        }

        private async Task AnalyzeAsync()
		{
			using (var scope = Container.BeginLifetimeScope())
			{
				var service = scope.Resolve<IInstancesAnalysisService>();
                await service.AnalyzeAsync(CancellationTokenSource.Token);
			}
		}
    }
}
