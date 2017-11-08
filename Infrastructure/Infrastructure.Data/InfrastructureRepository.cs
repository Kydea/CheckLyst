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

		public async Task<Departments> EditDepartmentAsync(long Company, Departments Department)
		{
            //TODO
			await SaveChangesAsync();
            return Department;
		}

        public async Task<PrototypeTypes> EditPrototypeTypeAsync(long Company, PrototypeTypes PrototypeType)
        {
            //TODO
            await SaveChangesAsync();
            return PrototypeType;
        }


        public async Task<Departments> GetDepartmentAsync(long Company, long ID)
        {
            return await Entities.Departments.SingleOrDefaultAsync(d => d.Company == Company && d.ID == ID);
        }

        public async Task<PagingResponse<List<TResult>>> GetDepartmentsAsync<TResult>(
            long companyId,
            Expression<Func<Departments, TResult>> select,
            string name, int currentPage, int itemsPerPage)
        {
            var Query = Entities.Departments
                .Where(d =>
                    d.Company == companyId
                    && (name == "" || d.Name != null && d.Name.ToUpper().Contains(name.ToUpper()))
                );

            var Data = await Query
                .OrderBy(d => d.Name).ThenBy(d => d.ID)
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

        public async Task<PrototypeTypes> GetPrototypeTypeAsync(long Company, long ID)
        {
            return await Entities.PrototypeTypes.SingleOrDefaultAsync(pt => pt.Company == Company && pt.ID == ID);
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
