using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Logic.Api.Messages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CheckListManager.Infrastructure.Logic.PrototypeTypes
{
    public interface IPrototypeTypesService: IService
    {
        Task<PrototypeTypesResponse> BindAsync(IDefaultIdentity identity, PrototypeTypesRequest form);
        Task<PagingResponse<List<PrototypeTypesRow>>> GetDataAsync(long company, PagingRequest<PrototypeTypesRequest> form);
        Task<PagingResponse<List<PrototypeTypesSyntheticRow>>> GetSyntheticDataAsync(long company, PagingRequest<PrototypeTypesRequest> form);
        Task<PrototypeTypesResponse> Add(IDefaultIdentity identity, PrototypeTypesRequest record);
        Task<PrototypeTypesResponse> Edit(IDefaultIdentity identity, PrototypeTypesRequest record);
        Task<PrototypeTypesResponse> Delete(IDefaultIdentity identity, long id);
    }
}
