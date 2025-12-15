using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using CheckListManager.Common.Logic.Terminal;
using CheckListManager.Common.Logic.Terminal.Configuration;
using CheckListManager.Common.Logic.Terminal.Scheduler;
using CheckListManager.Reports.Data;
using CheckListManager.Reports.Logic.Run;
using CheckListManager.Reports.Logic.Viewer;
using Newtonsoft.Json;
using NLog;

namespace CheckListManager.Infrastructure.Terminal
{
	public class ReportsWindowsService : TimerWindowsService
	{

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private CancellationTokenSource _serviceCancellatioTokenSource;

        public ReportsWindowsService(TerminalConfig config) : base(config)
		{
            Add(new IntervalSchedulerStrategy(RunAsync, 10));
		}

		protected async Task RunAsync(CancellationToken cancellationToken)
		{
			using (var scope = Container.BeginLifetimeScope())
			{
                var reportsRepository = scope.Resolve<IReportsRepository>();
                var reportviewerService = scope.Resolve<IReportViewerService>();
				var pendingRuns = await reportsRepository.GetPendingRunsAsync();
				foreach (var pendingRun in pendingRuns) 
				{
                    try
                    {
                        var reportRequest = JsonConvert.DeserializeObject<ReportRequest>(pendingRun.Parameters);
                        await reportviewerService.GetReportData(identity, reportRequest, cancellationToken);
                    }
                    catch (Exception exception)
                    {
                        //log error and continue with next run
                        Logger.Error(exception, $"Error executing report run ID {pendingRun.ID}");
                    }
                }
            }
        }
	}
}
