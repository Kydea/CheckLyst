using System.Threading.Tasks;
using CheckListManager.Common.Data;
using System.Collections.Generic;
using CheckListManager.Common.Logic.Api.Messages;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Data.Entity;

namespace CheckListManager.Infrastructure.Data
{
	public class InfrastructureRepository : Repository, IInfrastructureRepository
	{
		public InfrastructureRepository(RelationalDatabaseEntities entities) : base(entities)
		{
		}

		public async Task EditDepartmentAsync(Departments Department)
		{
            //TODO
			await SaveChangesAsync();
		}

        public async Task EditPrototypeTypeAsync(PrototypeTypes PrototypeType)
        {
            //TODO
            await SaveChangesAsync();
        }

        public async Task<PagingResponse<List<TResult>>> GetPrototypeTypesAsync<TResult>(
            long companyId, 
            Expression<Func<PrototypeTypes, TResult>> select, 
            long? department, string description, int currentPage, int itemsPerPage)
        {
            var Query = Entities.PrototypeTypes
                .Where(pt =>
                    pt.Company == companyId
                    && (department == null || pt.Department == department)
                    && (description == "" || pt.Description != null && pt.Description.ToUpper().Contains(description.ToUpper()))
                );

            var Data = await Query
                .OrderBy(pt => pt.Description).ThenBy(pt => pt.ID)
                .Select(select)
                .Skip((currentPage - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .ToListAsync();

            int Count = await Query.CountAsync();

            return new PagingResponse<List<TResult>>
            {
                Data = Data,
                Count = Count,
                ItemsPerPage = itemsPerPage,
                CurrentPage = currentPage
            };
        }
    }
}
