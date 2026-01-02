using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Reports.Data.Models;
using System.Threading;

namespace CheckListManager.Reports.Data
{
    public interface IReportsRepository : IRepository
    {
        Task<List<ReportSources>> GetReportSourcesAsync();

        Task<Common.Data.Reports> GetReportAsync(long company, long id, string user);
        Task<PagingResponse<List<Common.Data.Reports>>> GetReportsAsync(long company, string user, bool restrict, int? id, string code, string description, bool favourites, string state, int currentPage, int itemsPerPage);
        Task AddReportAsync(IDefaultIdentity identity, Common.Data.Reports report, List<string> groups, int? version);
        Task<Common.Data.Reports> EditReportAsync(IDefaultIdentity identity, Common.Data.Reports report, List<string> groups);
        Task DeleteReportAsync(IDefaultIdentity identity, long id);
        Task EnqueueReportRunAsync(IDefaultIdentity identity, long report, string format, string parameters);
        Task<PagingResponse<List<RunRecord>>> GetRunsAsync(long companyId, string user, long? report, DateTime? from, DateTime? to, int currentPage, int itemsPerPage);
        Task<List<ReportRuns>> GetPendingRunsAsync();
        Task<ReportRuns> GetRunAsync(long ID);
        Task DeleteRunAsync(DefaultIdentity identity, long iD);
        Task CompleteReportRunAsync(long id, byte[] data, Exception exception, CancellationToken cancellationToken);
        Task DeleteExpiredRunsAsync(CancellationToken cancellationToken);
        Task<List<WorkFlows>> GetWorkFlowsAsync(long company);
        Task<Common.Data.Reports> GetLastVersionAsync(long company, string code);
        Task ToggleFavourite(long company, string user, string code, bool value);
        Task<bool> IsFavouriteAsync(long company, string user, string code);
    }
}