using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Core.Web.Security;

namespace CheckListManager.Reports.Data
{
    public interface IReportsRepository : IRepository
    {
        Task<List<ReportSources>> GetReportSourcesAsync();

        Task<Common.Data.Reports> GetReportAsync(long company, string code);
        Task<PagingResponse<List<Common.Data.Reports>>> GetReportsAsync(long company, string code, string description, int currentPage, int itemsPerPage);
        Task AddReportAsync(IDefaultIdentity identity, Common.Data.Reports report, List<string> groups);
        Task<Common.Data.Reports> EditReportAsync(IDefaultIdentity identity, Common.Data.Reports report, List<string> groups);
        Task DeleteReportAsync(IDefaultIdentity identity, string code);
    }
}