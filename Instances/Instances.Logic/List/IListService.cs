using System.Collections.Generic;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Core.Web.Security;

namespace CheckListManager.Instances.Logic.List
{
    public interface IListService: IService
    {
        Task<ListResponse> BindAsync(DefaultIdentity identity, ListRequest form);

        Task<PagingResponse<List<ListRow>>> GetDataAsync(long company, PagingRequest<ListRequest> form);
    }
}
