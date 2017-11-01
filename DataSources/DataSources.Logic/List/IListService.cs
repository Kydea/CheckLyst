using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Logic.Api.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.DataSources.Logic.List
{
    public interface IListService: IService
    {
        Task<PagingResponse<List<ListRow>>> GetDataAsync(long Company, PagingRequest<ListRequest> form);
    }
}
