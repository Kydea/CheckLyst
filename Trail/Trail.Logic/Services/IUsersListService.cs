using System.Collections.Generic;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.Trail;

namespace CheckListManager.Trail.Logic.Services
{
    public interface IUsersListService: IService
    {
        Task<List<UsersListResponse>> GetDataAsync(long company, UsersListRequest form);
        Task<PagingResponse<List<UsersListResponse>>> GetDataAsync(long company, PagingRequest<UsersListRequest> form);
    }
}
