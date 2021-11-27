using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Prototypes.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CheckListManager.Prototypes.Logic.List
{
    public interface IListService: IService
    {
        Task<ListResponse> BindAsync(DefaultIdentity identity, PrototypeFilters form);
        Task<PagingResponse<List<PrototypeRecord>>> GetDataAsync(IDefaultIdentity identity, PagingRequest<PrototypeFilters> form);
        Task<List<SyntheticRecord>> GetSyntheticDataAsync(IDefaultIdentity identity);
    }
}
