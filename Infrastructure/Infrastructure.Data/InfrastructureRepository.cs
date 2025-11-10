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
using CheckListManager.Infrastructure.Data.ListItems;
using CheckListManager.Common.Data.ListItems;

namespace CheckListManager.Infrastructure.Data
{
    public class InfrastructureRepository : Repository, IInfrastructureRepository
    {
        public InfrastructureRepository(RelationalDatabaseEntities entities) : base(entities)
        {
        }

        private async Task<Companies> GetCompany(IDefaultIdentity identity, long id)
        {
            var Company = await Entities.Companies.SingleOrDefaultAsync(c => (identity.CompanyId == 1 || c.ID == identity.CompanyId) && c.ID == id) ?? throw new DatabaseException($"Company {id} not found");
            return Company;
        }

        private async Task<List<SettingValues>> GetSettingValues(long company) =>
            await Entities.SettingValues.Where(sv => sv.Company == company).ToListAsync();

        public async Task<Companies> EditCompanyAsync(IDefaultIdentity identity, Companies company, List<SettingValue> settings)
        {
            var ExistingCompany = await GetCompany(identity, company.ID);
            ExistingCompany.Name = company.Name;
            ExistingCompany.TimeZone = company.TimeZone;
            ExistingCompany.Online = company.Online;
            ExistingCompany.ModifiedOn = DateTimeOffset.UtcNow;
            ExistingCompany.ModifiedBy = identity.Code;
            var newSettingsDictionary = settings.ToDictionary(s => s.Code, s => s.Value);
            var oldSettingsDictionary = (await GetSettingValues(company.ID)).ToDictionary(s => s.Setting, s => s);
            foreach (var key in oldSettingsDictionary.Keys)
                if (key != "SmtpPassword" || newSettingsDictionary[key] != null)
                {
                    oldSettingsDictionary[key].Value = newSettingsDictionary[key] ?? "";
                    oldSettingsDictionary[key].ModifiedOn = DateTimeOffset.UtcNow;
                }

            foreach (var key in newSettingsDictionary.Keys)
                if (!oldSettingsDictionary.ContainsKey(key))
                    ExistingCompany.SettingValues.Add(new SettingValues { Company = company.ID, Setting = key, Value = newSettingsDictionary[key] ?? "", CreatedOn = DateTimeOffset.UtcNow });

            await SaveChangesAsync();
            return company;
        }

        public async Task SetCompanyOnlineAsync(IDefaultIdentity identity, long id)
        {
            var ExistingCompany = await GetCompany(identity, id);
            ExistingCompany.Online = true;
            ExistingCompany.ModifiedOn = DateTimeOffset.UtcNow;
            ExistingCompany.ModifiedBy = identity.Code;
            await SaveChangesAsync();
        }

        public async Task<Companies> AddCompanyAsync(IDefaultIdentity identity, Companies company, Users configurator, List<SettingValue> settings)
        {
            company.CreatedOn = DateTimeOffset.UtcNow;
            company.CreatedBy = identity.Code;
            configurator.UserFunctions.Add(new UserFunctions
            {
                FunctionGroup = "Users",
                Function = "View",
                CreatedOn = DateTimeOffset.UtcNow
            });
            configurator.UserFunctions.Add(new UserFunctions
            {
                FunctionGroup = "Users",
                Function = "Edit",
                CreatedOn = DateTimeOffset.UtcNow
            });
            company.Users.Add(configurator);
            foreach (var setting in settings)
                company.SettingValues.Add(new SettingValues { Setting = setting.Code, Value = setting.Value ?? "", CreatedOn = DateTimeOffset.UtcNow });
            Entities.Companies.Add(company);
            await SaveChangesAsync();

            if (!await Call<bool>("\"Security\".\"CreateReportUser\"", new Parameter("company", company.ID))) throw new Exception("Unexpected result creating report user");
            return company;
        }

        public async Task<List<SettingValue>> GetSettings(IDefaultIdentity identity, long? company) =>
            await Entities
                .Settings.OrderBy(s => s.Code)
                .Select(s => 
                    s.SettingValues.Where(sv => sv.Company == company)
                    .Select(sv => new SettingValue { Code = sv.Setting, Value = sv.Setting == "SmtpPassword" ? null : sv.Value }).FirstOrDefault() ?? new SettingValue { Code = s.Code, Value = null })
                .ToListAsync();

        private async Task<Departments> GetDepartment(IDefaultIdentity identity, long id)
        {
            var Department = await Entities.Departments.SingleOrDefaultAsync(d => d.Company == identity.CompanyId && d.ID == id) ?? throw new DatabaseException($"Department {id} not found");
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
            var PrototypeType = await Entities.PrototypeTypes.SingleOrDefaultAsync(d => d.Company == identity.CompanyId && d.ID == id) ?? throw new DatabaseException($"Prototype type {id} not found");
            return PrototypeType;
        }

        public async Task<PrototypeTypes> EditPrototypeTypeAsync(IDefaultIdentity identity, PrototypeTypes prototypeType)
        {
            var ExistingPrototypeType = await GetPrototypeTypeAsync(identity, prototypeType.ID);
            ExistingPrototypeType.Name = prototypeType.Name;
            ExistingPrototypeType.LifeCycle = prototypeType.LifeCycle;
            ExistingPrototypeType.ApprovationWorkFlow = prototypeType.ApprovationWorkFlow;
            ExistingPrototypeType.ObsolescenceWorkFlow = prototypeType.ObsolescenceWorkFlow;
            ExistingPrototypeType.PrototypesFTPServerEnabled = prototypeType.PrototypesFTPServerEnabled;
            ExistingPrototypeType.PrototypesFTPServer = prototypeType.PrototypesFTPServer;
            ExistingPrototypeType.InstancesFTPServerEnabled = prototypeType.InstancesFTPServerEnabled;
            ExistingPrototypeType.InstancesFTPServer = prototypeType.InstancesFTPServer;
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

        public async Task<FTPServers> GetFTPServerAsync(IDefaultIdentity identity, long id)
        {
            var FTPServer = await Entities.FTPServers.SingleOrDefaultAsync(d => d.Company == identity.CompanyId && d.ID == id) ?? throw new DatabaseException($"FTP server {id} not found");
            return FTPServer;
        }

        public async Task<FTPServers> EditFTPServerAsync(IDefaultIdentity identity, FTPServers ftpServer)
        {
            var ExistingFTPServer = await GetFTPServerAsync(identity, ftpServer.ID);
            ExistingFTPServer.Name = ftpServer.Name;
            ExistingFTPServer.Protocol = ftpServer.Protocol;
            ExistingFTPServer.Host = ftpServer.Host;
            ExistingFTPServer.Port = ftpServer.Port;
            ExistingFTPServer.Folder = ftpServer.Folder;
            ExistingFTPServer.UserName = ftpServer.UserName;
            ExistingFTPServer.Password = ftpServer.Password;
            ExistingFTPServer.Passive = ftpServer.Passive && ftpServer.Protocol == "FTP";
            ExistingFTPServer.SSL = ftpServer.SSL && ftpServer.Protocol == "FTP";
            ExistingFTPServer.Key = ftpServer.Protocol == "SFTP" ? ftpServer.Key : null;
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
            Entities.FTPServers.Remove(await GetFTPServerAsync(identity, id));
            await SaveChangesAsync();
        }
        public async Task<PagingResponse<List<TResult>>> GetCompaniesAsync<TResult>(
            IDefaultIdentity identity,
            Expression<Func<Companies, TResult>> select,
            string name, int currentPage, int itemsPerPage)
        {
            IQueryable<TResult> Query = Entities.Companies
                .Where(c =>
                    (identity.CompanyId == 1 || c.ID == identity.CompanyId)
                    && (name == "" || c.Name != null && c.Name.ToUpper().Contains(name.ToUpper()))
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
            long? department, string name,
            int currentPage, int itemsPerPage)
        {
            var Query = Entities.PrototypeTypes
                .Where(pt =>
                    pt.Company == identity.CompanyId
                    && (department == null || pt.Department == department)
                    && (name == "" || pt.Name != null && pt.Name.ToUpper().Contains(name.ToUpper()))
                )
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

        public async Task<List<WorkFlows>> GetWorkFlows(long companyId) =>
            await Entities.WorkFlows
                .Where(wf => wf.Company == companyId)
                .OrderBy(wf => wf.Description).ThenBy(wf => wf.ID)
                .ToListAsync();

        public async Task<List<string>> GetTimeZonesAsync() =>
            await Entities.TimeZones
                .OrderBy(wf => wf.Code)
                .Select(wf => wf.Code)
                .ToListAsync();
    }
}
