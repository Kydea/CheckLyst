using System.Threading.Tasks;
using CheckListManager.Common.Data;
using System.Collections.Generic;
using CheckListManager.Common.Logic.Api.Messages;
using System;
using System.Linq.Expressions;

namespace CheckListManager.Infrastructure.Data
{
	public interface IInfrastructureRepository : IRepository
	{
		Task<Departments> EditDepartmentAsync(long Company, Departments Department);

        Task<PrototypeTypes> EditPrototypeTypeAsync(long Company, PrototypeTypes PrototypeType);

        Task<Departments> GetDepartmentAsync(long companyId, long id);

        Task<PagingResponse<List<TResult>>> GetDepartmentsAsync<TResult>(
            long companyId,
            Expression<Func<Departments, TResult>> select,
            string name, int currentPage, int itemsPerPage);

        Task<PrototypeTypes> GetPrototypeTypeAsync(long companyId, long id);

        Task<PagingResponse<List<TResult>>> GetPrototypeTypesAsync<TResult>(
            long companyId,
            Expression<Func<PrototypeTypes, TResult>> select,
            long? department, string description, int currentPage, int itemsPerPage);
    }
}
