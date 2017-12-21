using System.Threading.Tasks;
using CheckListManager.Common.Data;
using System.Collections.Generic;
using CheckListManager.Common.Logic.Api.Messages;
using System;
using System.Linq.Expressions;
using CheckListManager.Common.Core.Web.Security;

namespace CheckListManager.Infrastructure.Data
{
	public interface IInfrastructureRepository : IRepository
	{
		Task<Departments> EditDepartmentAsync(IDefaultIdentity identity, Departments department);

        Task<Departments> AddDepartmentAsync(IDefaultIdentity identity, Departments department);

        Task DeleteDepartmentAsync(IDefaultIdentity identity, long id);

        Task<PrototypeTypes> EditPrototypeTypeAsync(IDefaultIdentity identity, PrototypeTypes prototypeType);

        Task<PrototypeTypes> AddPrototypeTypeAsync(IDefaultIdentity identity, PrototypeTypes prototypeType);

        Task DeletePrototypeTypeAsync(IDefaultIdentity identity, long id);

        Task<PagingResponse<List<TResult>>> GetDepartmentsAsync<TResult>(
            long companyId,
            Expression<Func<Departments, TResult>> select,
            string name, int currentPage, int itemsPerPage);

        Task<PagingResponse<List<TResult>>> GetPrototypeTypesAsync<TResult>(
            long companyId,
            Expression<Func<PrototypeTypes, TResult>> select,
            long? department, string name, int currentPage, int itemsPerPage);

        Task<List<WorkFlows>> GetWorkFlows(long companyId);
    }
}
