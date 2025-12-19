using System;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Core.Network;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Cache;
using CheckListManager.Common.Logic.DataEngine;
using CheckListManager.Common.Logic.Models;
using CheckListManager.Common.Logic.Models.Expressions.Functions;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;
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

        private IntervalSchedulerStrategy reportProcessStrategy;

        public ReportsWindowsService(TerminalConfig config) : base(config)
        {
            Add(reportProcessStrategy = new IntervalSchedulerStrategy(ProcessReportsAsync, 10));
            Add(new ComputedSchedulerStrategy(DeleteExpiredReportsAsync, new[] { 0 }, new[] { 0 }));
        }

        private async Task ProcessReportsAsync(CancellationToken cancellationToken)
        {
            using (var scope = Container.BeginLifetimeScope())
            {
                var reportsRepository = scope.Resolve<IReportsRepository>();
                var reportviewerService = scope.Resolve<IReportViewerService>();
                var client = scope.Resolve<IServiceClient>();
                var pendingRuns = await reportsRepository.GetPendingRunsAsync();
                var redisCache = scope.Resolve<IRedisCache>();
                if (pendingRuns.Count == 0)
                    reportProcessStrategy.Timer.Interval = Math.Min(reportProcessStrategy.Timer.Interval * 2, 8000);
                else
                {
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

                            var token = pendingRun.User + ":" + Guid.NewGuid();
                            var user = new User
                            {
                                Company = pendingRun.Company,
                                Code = pendingRun.User
                            };
                            await redisCache.SetAsync($"Token:{token}", user, "Service", new TimeSpan(0, 1, 0));
                            await client.GetAsync("Infrastructure/Notifications/Invalidate?token=" + token);
                        }
                        catch (Exception exception)
                        {
                            Logger.Error(exception, $"Error executing report run {pendingRun.ID}");
                            await reportsRepository.CompleteReportRunAsync(pendingRun.ID, null, exception, cancellationToken);
                        }
                    }
                    reportProcessStrategy.Timer.Interval = 1000;
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
