using System.Collections.Generic;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.Trail;

namespace CheckListManager.Trail.Logic.Services
{
    public interface IListService: IService
    {
        Task<List<TrailRow>> GetDataAsync(long Company, ListRequest form);
        Task<PagingResponse<List<TrailRow>>> GetDataAsync(long Company, PagingRequest<ListRequest> form);
    }
}
