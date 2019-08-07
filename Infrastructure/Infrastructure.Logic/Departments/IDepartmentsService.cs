using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Core.Web.Security;
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
        Task<PagingResponse<List<DepartmentsRow>>> GetDataAsync(IDefaultIdentity Identity, PagingRequest<DepartmentsRequest> form);

        Task<DepartmentsResponse> AddOrEdit(IDefaultIdentity Identity, DepartmentsRow record, bool IsNew);

        Task<DepartmentsResponse> Delete(IDefaultIdentity Identity, DepartmentsRow record);
    }
}
