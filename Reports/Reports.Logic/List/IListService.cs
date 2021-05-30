using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Logic.Api.Messages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CheckListManager.Reports.Logic.List
{
    public interface IListService: IService
    {
        Task<PagingResponse<List<ListRow>>> GetDataAsync(IDefaultIdentity identity, PagingRequest<ListRequest> form);
    }
}
