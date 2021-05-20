using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Core.Web.Security;
using System.Threading.Tasks;

namespace CheckListManager.Reports.Logic.Detail
{
    public interface IDetailService: IService
    {
        Task<DetailResponse> AddAsync(IDefaultIdentity identity, DetailRequest record);

        Task<DetailResponse> EditAsync(IDefaultIdentity identity, DetailRequest record);

        Task<DetailResponse> DeleteAsync(IDefaultIdentity identity, DetailRequest record);
    }
}
