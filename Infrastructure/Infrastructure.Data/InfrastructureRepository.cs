using System.Threading.Tasks;
using CheckListManager.Common.Data;
using System.Collections.Generic;
using CheckListManager.Common.Logic.Api.Messages;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Data.Entity;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Data.Exceptions;

namespace CheckListManager.Infrastructure.Data
{
	public class InfrastructureRepository : Repository, IInfrastructureRepository
	{
		public InfrastructureRepository(RelationalDatabaseEntities entities) : base(entities)
		{
		}

        private async Task<Departments> GetDepartment(DefaultIdentity identity, long id)
        {
            var Department = await Entities.Departments.SingleOrDefaultAsync(d => d.Company == identity.CompanyId && d.ID == id);
            if (Department == null) throw new DatabaseException($"Department {id} not found");
            return Department;
        }

        public async Task<Departments> EditDepartmentAsync(DefaultIdentity identity, Departments department)
		{
            var ExistingDepartment = await GetDepartment(identity, department.ID);
            ExistingDepartment.ModifiedOn = DateTimeOffset.UtcNow;
            ExistingDepartment.ModifiedBy = identity.Code;
            ExistingDepartment.Name = department.Name;
            await SaveChangesAsync();
            return department;
		}

        public async Task<Departments> AddDepartmentAsync(DefaultIdentity identity, Departments department)
        {
            department.Company = identity.CompanyId;
            department.CreatedOn = DateTimeOffset.UtcNow;
            department.CreatedBy = identity.Code;
            Entities.Departments.Add(department);
            await SaveChangesAsync();
            return department;
        }

        public async Task DeleteDepartmentAsync(DefaultIdentity identity, long id)
        {
            Entities.Departments.Remove(await GetDepartment(identity, id));
            await SaveChangesAsync();
        }

        private async Task<PrototypeTypes> GetPrototypeTypeAsync(DefaultIdentity identity, long id)
        {
            var PrototypeType = await Entities.PrototypeTypes.SingleOrDefaultAsync(d => d.Company == identity.CompanyId && d.ID == id);
            if (PrototypeType == null) throw new DatabaseException($"Prototype type {id} not found");
            return PrototypeType;
        }

        public async Task<PrototypeTypes> EditPrototypeTypeAsync(DefaultIdentity identity, PrototypeTypes prototypeType)
        {
            var ExistingPrototypeType = await GetPrototypeTypeAsync(identity, prototypeType.ID);
            ExistingPrototypeType.ModifiedOn = DateTimeOffset.UtcNow;
            ExistingPrototypeType.ModifiedBy = identity.Code;
            ExistingPrototypeType.Description = prototypeType.Description;
            ExistingPrototypeType.ApprovationWorkFlow = prototypeType.ApprovationWorkFlow;
            ExistingPrototypeType.ObsolescenceWorkFlow = prototypeType.ObsolescenceWorkFlow;
            await SaveChangesAsync();
            return prototypeType;
        }

        public async Task<PrototypeTypes> AddPrototypeTypeAsync(DefaultIdentity identity, PrototypeTypes prototypeType)
        {
            prototypeType.Company = identity.CompanyId;
            prototypeType.CreatedOn = DateTimeOffset.UtcNow;
            prototypeType.CreatedBy = identity.Code;
            Entities.PrototypeTypes.Add(prototypeType);
            await SaveChangesAsync();
            return prototypeType;
        }

        public async Task DeletePrototypeTypeAsync(DefaultIdentity identity, long id)
        {
            Entities.PrototypeTypes.Remove(await GetPrototypeTypeAsync(identity, id));
            await SaveChangesAsync();
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
