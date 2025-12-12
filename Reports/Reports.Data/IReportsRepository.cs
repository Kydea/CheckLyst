using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Reports.Data.Models;

namespace CheckListManager.Reports.Data
{
    public interface IReportsRepository : IRepository
    {
        Task<List<ReportSources>> GetReportSourcesAsync();

        Task<Common.Data.Reports> GetReportAsync(long company, long id, string user);
        Task<PagingResponse<List<Common.Data.Reports>>> GetReportsAsync(long company, string user, int? id, string code, string description, int currentPage, int itemsPerPage);
        Task AddReportAsync(IDefaultIdentity identity, Common.Data.Reports report, List<string> groups);
        Task<Common.Data.Reports> EditReportAsync(IDefaultIdentity identity, Common.Data.Reports report, List<string> groups);
        Task DeleteReportAsync(IDefaultIdentity identity, long id);
        Task EnqueueReportRunAsync(IDefaultIdentity identity, long report, string parameters);
        Task<long> CountRunsAsync(IDefaultIdentity identity);
        Task<PagingResponse<List<RunRecord>>> GetRunsAsync(long companyId, string user, long? report, DateTime? from, DateTime? to, int currentPage, int itemsPerPage);
        Task DeleteRunAsync(DefaultIdentity identity, long iD);
    }
}