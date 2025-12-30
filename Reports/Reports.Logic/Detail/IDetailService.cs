using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Logic.Api.Messages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CheckListManager.Reports.Logic.Detail
{
    public interface IDetailService: IService
    {
        Task<DetailResponse> SaveReportAsync(IDefaultIdentity identity, DetailRequest request);
        Task<DetailResponse> DeleteReportAsync(IDefaultIdentity identity, DetailRequest request);
        Task<DetailResponse> VerifyReportAsync(IDefaultIdentity identity, DetailRequest request);
        Task<DetailResponse> LoadReportAsync(long companyId, DetailRequest request);
        Task<DetailResponse> SetStateAsync(IDefaultIdentity identity, DetailRequest request);

        Task<List<ReportSource>> GetReportSourcesAsync();
        Task<List<string>> GetColumnsAsync(IDefaultIdentity identity, string query, Dictionary<string, string> parameters);
    }
}
