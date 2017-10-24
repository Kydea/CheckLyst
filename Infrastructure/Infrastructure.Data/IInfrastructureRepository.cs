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
		Task EditDepartmentAsync(Departments Department);

        Task EditPrototypeTypeAsync(PrototypeTypes PrototypeType);

        Task<PagingResponse<List<TResult>>> GetPrototypeTypesAsync<TResult>(
            long companyId,
            Expression<Func<PrototypeTypes, TResult>> select,
            long? department, string description, int currentPage, int itemsPerPage);
    }
}
