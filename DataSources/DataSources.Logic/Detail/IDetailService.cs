using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Core.Web.Security;
using System.Threading.Tasks;

namespace CheckListManager.DataSources.Logic.Detail
{
    public interface IDetailService: IService
    {
        Task<DetailResponse> AddAsync(DefaultIdentity identity, DetailRequest record);

        Task<DetailResponse> EditAsync(DefaultIdentity identity, DetailRequest record);

        Task<DetailResponse> DeleteAsync(DefaultIdentity identity, string code);
    }
}
