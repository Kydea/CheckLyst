using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.Trail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Infrastructure.Logic.Departments
{
    public interface IDepartmentsService : IService
    {
        Task<PagingResponse<List<DepartmentsRow>>> GetDataAsync(long Company, PagingRequest<DepartmentsRequest> form);

        Task<DepartmentsResponse> Edit(long Company, DepartmentsRow record);
    }
}
