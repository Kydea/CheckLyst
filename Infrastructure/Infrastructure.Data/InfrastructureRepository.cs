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

        private async Task<Departments> GetDepartment(IDefaultIdentity identity, long id)
        {
            var Department = await Entities.Departments.SingleOrDefaultAsync(d => d.Company == identity.CompanyId && d.ID == id);
            if (Department == null) throw new DatabaseException($"Department {id} not found");
            return Department;
        }

        public async Task<Departments> EditDepartmentAsync(IDefaultIdentity identity, Departments department)
		{
            var ExistingDepartment = await GetDepartment(identity, department.ID);
            ExistingDepartment.ModifiedOn = DateTimeOffset.UtcNow;
            ExistingDepartment.ModifiedBy = identity.Code;
            ExistingDepartment.Name = department.Name;
            await SaveChangesAsync();
            return department;
		}

        public async Task<Departments> AddDepartmentAsync(IDefaultIdentity identity, Departments department)
        {
            department.Company = identity.CompanyId;
            department.CreatedOn = DateTimeOffset.UtcNow;
            department.CreatedBy = identity.Code;
            Entities.Departments.Add(department);
            await SaveChangesAsync();
            return department;
        }

        public async Task DeleteDepartmentAsync(IDefaultIdentity identity, long id)
        {
            Entities.Departments.Remove(await GetDepartment(identity, id));
            await SaveChangesAsync();
        }

        private async Task<PrototypeTypes> GetPrototypeTypeAsync(IDefaultIdentity identity, long id)
        {
            var PrototypeType = await Entities.PrototypeTypes.SingleOrDefaultAsync(d => d.Company == identity.CompanyId && d.ID == id);
            if (PrototypeType == null) throw new DatabaseException($"Prototype type {id} not found");
            return PrototypeType;
        }

        public async Task<PrototypeTypes> EditPrototypeTypeAsync(IDefaultIdentity identity, PrototypeTypes prototypeType)
        {
            var ExistingPrototypeType = await GetPrototypeTypeAsync(identity, prototypeType.ID);
            ExistingPrototypeType.Name = prototypeType.Name;
            ExistingPrototypeType.ApprovationWorkFlow = prototypeType.ApprovationWorkFlow;
            ExistingPrototypeType.ObsolescenceWorkFlow = prototypeType.ObsolescenceWorkFlow;
            ExistingPrototypeType.ModifiedOn = DateTimeOffset.UtcNow;
            ExistingPrototypeType.ModifiedBy = identity.Code;
            await SaveChangesAsync();
            return ExistingPrototypeType;
        }

        public async Task<PrototypeTypes> AddPrototypeTypeAsync(IDefaultIdentity identity, PrototypeTypes prototypeType)
        {
            prototypeType.Company = identity.CompanyId;
            prototypeType.CreatedOn = DateTimeOffset.UtcNow;
            prototypeType.CreatedBy = identity.Code;
            Entities.PrototypeTypes.Add(prototypeType);
            await SaveChangesAsync();
            return prototypeType;
        }

        public async Task DeletePrototypeTypeAsync(IDefaultIdentity identity, long id)
        {
            Entities.PrototypeTypes.Remove(await GetPrototypeTypeAsync(identity, id));
            await SaveChangesAsync();
        }

        private async Task<FTPServers> GetFTPServerAsync(IDefaultIdentity identity, long id)
        {
            var FTPServer = await Entities.FTPServers.SingleOrDefaultAsync(d => d.Company == identity.CompanyId && d.ID == id);
            if (FTPServer == null) throw new DatabaseException($"FTP server {id} not found");
            return FTPServer;
        }

        public async Task<FTPServers> EditFTPServerAsync(IDefaultIdentity identity, FTPServers ftpServer)
        {
            var ExistingFTPServer = await GetFTPServerAsync(identity, ftpServer.ID);
            ExistingFTPServer.Name = ftpServer.Name;
            ExistingFTPServer.Host = ftpServer.Host;
            ExistingFTPServer.Port = ftpServer.Port;
            ExistingFTPServer.Folder = ftpServer.Folder;
            ExistingFTPServer.UserName = ftpServer.UserName;
            ExistingFTPServer.Password = ftpServer.Password;
            ExistingFTPServer.Passive = ftpServer.Passive;
            ExistingFTPServer.SSL = ftpServer.SSL;
            ExistingFTPServer.PrototypesDefault = ftpServer.PrototypesDefault;
            ExistingFTPServer.InstancesDefault = ftpServer.InstancesDefault;
            ExistingFTPServer.ModifiedOn = DateTimeOffset.UtcNow;
            ExistingFTPServer.ModifiedBy = identity.Code;
            await SaveChangesAsync();
            return ExistingFTPServer;
        }

        public async Task<FTPServers> AddFTPServerAsync(IDefaultIdentity identity, FTPServers ftpServer)
        {
            ftpServer.Company = identity.CompanyId;
            ftpServer.CreatedOn = DateTimeOffset.UtcNow;
            ftpServer.CreatedBy = identity.Code;
            Entities.FTPServers.Add(ftpServer);
            await SaveChangesAsync();
            return ftpServer;
        }

        public async Task DeleteFTPServerAsync(IDefaultIdentity identity, long id)
        {
            Entities.PrototypeTypes.Remove(await GetPrototypeTypeAsync(identity, id));
            await SaveChangesAsync();
        }
        public async Task<PagingResponse<List<TResult>>> GetDepartmentsAsync<TResult>(
            IDefaultIdentity identity,
            Expression<Func<Departments, TResult>> select,
            string name, int currentPage, int itemsPerPage)
        {
            IQueryable<TResult> Query = Entities.Departments
                .Where(d =>
                    d.Company == identity.CompanyId
                    && (name == "" || d.Name != null && d.Name.ToUpper().Contains(name.ToUpper()))
                )
                .OrderBy(d => d.Name).ThenBy(d => d.ID)
                .Select(select);

            var Data = await Query
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
            IDefaultIdentity identity, 
            Expression<Func<PrototypeTypes, TResult>> select, 
            long? department, string name, string functionGroup,
            int currentPage, int itemsPerPage, string function = "View")
        {
            IQueryable<PrototypeTypes> PrototypeTypes = Entities.PrototypeTypes
                .Where(pt =>
                    pt.Company == identity.CompanyId
                    && (department == null || pt.Department == department)
                    && (name == "" || pt.Name != null && pt.Name.ToUpper().Contains(name.ToUpper()))
                );

            if (functionGroup != null)
                PrototypeTypes = PrototypeTypes.Where(pt => pt.UserFunctions.Any(uf =>
                    uf.Company == identity.CompanyId
                    && uf.User == identity.Code
                    && uf.FunctionGroup == functionGroup
                    && uf.Function == function
                ) || pt.Departments.UserFunctions.Any(uf =>
                    uf.Company == identity.CompanyId
                    && uf.User == identity.Code
                    && uf.FunctionGroup == functionGroup
                    && uf.Function == function
                    && uf.PrototypeType == null
                ));


            IQueryable<TResult> Query = PrototypeTypes
                .OrderBy(pt => pt.Department).ThenBy(pt => pt.Name).ThenBy(pt => pt.ID)
                .Select(select);

            var Data = await Query
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

        public async Task<PagingResponse<List<TResult>>> GetFTPServersAsync<TResult>(
            IDefaultIdentity identity,
            Expression<Func<FTPServers, TResult>> select,
            string name, string host, int currentPage, int itemsPerPage)
        {
            IQueryable<FTPServers> FTPServers = Entities.FTPServers
                .Where(ftps =>
                    ftps.Company == identity.CompanyId
                    && (name == "" || ftps.Name != null && ftps.Name.ToUpper().Contains(name.ToUpper()))
                    && (host == "" || ftps.Host != null && ftps.Host.ToUpper().Contains(host.ToUpper()))
                );

            IQueryable<TResult> Query = FTPServers
                .OrderBy(pt => pt.Name).ThenBy(pt => pt.ID)
                .Select(select);

            var Data = await Query
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

        public async Task<List<WorkFlows>> GetWorkFlows(long companyId)
        {
            return await Entities.WorkFlows
                .Where(wf => wf.Company == companyId)
                .OrderBy(wf => wf.Description).ThenBy(wf => wf.ID)
                .ToListAsync();
        }

    }
}
