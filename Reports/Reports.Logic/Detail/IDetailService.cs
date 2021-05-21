using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Core.Web.Security;
using System.Threading.Tasks;

namespace CheckListManager.Reports.Logic.Detail
{
    public interface IDetailService: IService
    {
        Task<DetailResponse> AddReportAsync(IDefaultIdentity identity, DetailRequest request);
        Task<DetailResponse> EditReportAsync(IDefaultIdentity identity, DetailRequest request);
        Task<DetailResponse> DeleteReportAsync(IDefaultIdentity identity, DetailRequest request);
        Task<DetailResponse> LoadReportAsync(long companyId, DetailRequest request);
    }
}
