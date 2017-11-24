using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Core.Web.Security;
using System.Threading.Tasks;

namespace CheckListManager.DataSources.Logic.Detail
{
    public interface IDetailService: IService
    {
        Task<DetailResponse> Add(DefaultIdentity identity, DetailRequest record);

        Task<DetailResponse> Edit(DefaultIdentity identity, DetailRequest record);

        Task<DetailResponse> Delete(DefaultIdentity identity, string code);
    }
}
