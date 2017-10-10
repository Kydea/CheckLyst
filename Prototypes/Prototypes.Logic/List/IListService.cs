using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Logic.Api.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Prototypes.Logic.List
{
    public interface IListService: IService
    {
        Task<ListResponse> GetResponse(long Company, ListRequest form);

        Task<List<ListRow>> GetPrototypes(long Company, PagingRequest<ListRequest> form);
    }
}
