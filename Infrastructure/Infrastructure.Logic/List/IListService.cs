using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.Trail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Infrastructure.Logic.List
{
    public interface IListService: IService
    {
        Task<ListResponse> BindAsync(long Company, ListRequest form);

        Task<PagingResponse<List<ListRow>>> GetDataAsync(long Company, PagingRequest<ListRequest> form);
    }
}
