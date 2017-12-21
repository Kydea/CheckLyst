using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Logic.Api.Messages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CheckListManager.Prototypes.Logic.List
{
    public interface IListService: IService
    {
        Task<ListResponse> BindAsync(DefaultIdentity identity, ListRequest form);

        Task<PagingResponse<List<ListRow>>> GetDataAsync(long company, PagingRequest<ListRequest> form);
    }
}
