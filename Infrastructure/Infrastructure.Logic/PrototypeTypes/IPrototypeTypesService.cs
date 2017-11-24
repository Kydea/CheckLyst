using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.Trail;
using CheckListManager.Infrastructure.Logic.Departments;
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

        Task<PagingResponse<List<PrototypeTypesRow>>> GetDataAsync(long company, PagingRequest<PrototypeTypesRequest> form);

        Task<PrototypeTypesResponse> Add(DefaultIdentity identity, PrototypeTypesRequest record);

        Task<PrototypeTypesResponse> Edit(DefaultIdentity identity, PrototypeTypesRequest record);

        Task<PrototypeTypesResponse> Delete(DefaultIdentity identity, long id);
    }
}
