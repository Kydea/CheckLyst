using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.Trail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Infrastructure.Logic.PrototypeTypes
{
    public interface IPrototypeTypesService: IService
    {
        Task<PrototypeTypesResponse> BindAsync(long Company, PrototypeTypesRequest form);

        Task<PagingResponse<List<PrototypeTypesRow>>> GetDataAsync(long Company, PagingRequest<PrototypeTypesRequest> form);
    }
}
