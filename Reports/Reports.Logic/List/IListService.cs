using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Logic.Api.Messages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CheckListManager.Reports.Logic.List
{
    public interface IListService: IService
    {
        Task<PagingResponse<List<ListRow>>> GetDataAsync(long Company, PagingRequest<ListRequest> form);
    }
}
