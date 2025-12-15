using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Logic.Models.Expressions.Functions;
using CheckListManager.Common.Logic.Terminal;
using CheckListManager.Common.Logic.Terminal.Configuration;
using CheckListManager.Common.Logic.Terminal.Scheduler;
using CheckListManager.Reports.Data;
using CheckListManager.Reports.Logic;
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
            Add(new IntervalSchedulerStrategy(ProcessReportsAsync, 10));
            Add(new ComputedSchedulerStrategy(DeleteExpiredReportsAsync, new[] { 0 }, new[] { 0 }));
        }

        private async Task ProcessReportsAsync(CancellationToken cancellationToken)
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
                        var response = await reportviewerService.GetReportData(new ReportIdentity
                        {
                            CompanyId = pendingRun.Company,
                            Code = pendingRun.User
                        }, reportRequest, cancellationToken);
                        var data = CompressionHelper.SerializeToCompressedBytes(response, useGZipCompression: true);

                        await reportsRepository.CompleteReportRunAsync(pendingRun.ID, data, null, cancellationToken);
                    }
                    catch (Exception exception)
                    {
                        Logger.Error(exception, $"Error executing report run {pendingRun.ID}");
                        await reportsRepository.CompleteReportRunAsync(pendingRun.ID, null, exception, cancellationToken);
                    }
                }
            }
        }

        private async Task DeleteExpiredReportsAsync(CancellationToken cancellationToken)
        {
            using (var scope = Container.BeginLifetimeScope())
            {
                var reportsRepository = scope.Resolve<IReportsRepository>();
                await reportsRepository.DeleteExpiredRunsAsync(cancellationToken);
            }
        }
    }
}
