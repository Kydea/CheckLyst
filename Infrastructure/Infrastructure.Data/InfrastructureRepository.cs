using System.Threading.Tasks;
using CheckListManager.Common.Data;
using System.Collections.Generic;
using CheckListManager.Common.Logic.Api.Messages;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Data.Entity;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Infrastructure.Data.ListItems;
using CheckListManager.Common.Core.Exceptions;
using CheckListManager.Common.Logic.Models.Notifications;
using CheckListManager.Common.Logic.Models;
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
                if ((key != "SmtpPassword" && key != "LicenseSignature") || newSettingsDictionary[key] != null)
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

        public Task<long> CountCompaniesAsync(bool? online) => Entities.Companies.LongCountAsync(c => online == true && c.Online || online == false && !c.Online || online == null);

        public async Task<List<SettingValue>> GetSettings(IDefaultIdentity identity, long? company) =>
            await Entities
                .Settings.OrderBy(s => s.Code)
                .Select(s =>
                    s.SettingValues.Where(sv => sv.Company == company)
                    .Select(sv => new SettingValue { Code = sv.Setting, Value = sv.Setting == "SmtpPassword" || sv.Setting == "LicenseSignature" ? null : sv.Value }).FirstOrDefault() ?? new SettingValue { Code = s.Code, Value = null })
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

        public Task<long> CountPrototypeTypesAsync(long company) => Entities.PrototypeTypes.LongCountAsync(pt => pt.Company == company);

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

        public async Task<List<WorkFlows>> GetWorkFlows(long company) =>
            await Entities.WorkFlows
                .Where(wf => wf.Company == company)
                .OrderBy(wf => wf.Description).ThenBy(wf => wf.ID)
                .ToListAsync();

        private async Task<long> CountReportsToSign(long company, string user)
        {
            var userRecord = await Entities.Users.SingleAsync(u => u.Company == company && u.Code == user);
            var result = Entities.WorkFlowApplications
                .Join(
                    Entities.Reports,
                    application => application.Report,
                    report => report.ID,
                    (application, report) => new { application, report }
                )
                .Join(
                    Entities.WorkFlowApplicationSteps,
                    x => x.application.ID,
                    step => step.WorkFlowApplication,
                    (x, step) => new { x.report, x.application, step }
                )
                .Join(
                    Entities.WorkFlowApplicationStepTags,
                    x => x.step.ID,
                    tag => tag.WorkFlowApplicationStep,
                    (x, tag) => new { x.report, x.application, x.step, tag }
                )
                .Where(x =>
                    !x.application.Approved.HasValue

                    && x.step.User == null

                    && !Entities.WorkFlowApplicationSteps.Any(ps =>
                        ps.WorkFlowApplication == x.step.WorkFlowApplication
                        && ps.User == null
                        && x.step.Step > ps.Step
                    )

                    && !Entities.WorkFlowApplicationSteps.Any(cs =>
                        cs.WorkFlowApplication == x.step.WorkFlowApplication
                        && cs.User == user
                        && x.step.Step == cs.Step
                    )

                    && (
                        x.tag.Tag == null ||
                        userRecord.ReportWorkFlowLevel == x.tag.Tag
                    )
                );

            return await result.Select(x => x.report.ID).Distinct().LongCountAsync();
        }

        private Task<long> CountInstancesToSign(long company, string user)
        {
            var result = Entities.WorkFlowApplications
                .Join(
                    Entities.Instances,
                    application => application.Instance,
                    intance => intance.ID,
                    (application, instance) => new { application, instance }
                )
                .Join(
                    Entities.WorkFlowApplicationSteps,
                    x => x.application.ID,
                    step => step.WorkFlowApplication,
                    (x, step) => new { x.instance, x.application, step }
                )
                .Join(
                    Entities.WorkFlowApplicationStepTags,
                    x => x.step.ID,
                    tag => tag.WorkFlowApplicationStep,
                    (x, tag) => new { x.instance, x.application, x.step, tag }
                )
                .Where(x =>
                    !x.application.Approved.HasValue

                    && x.step.User == null

                    && !Entities.WorkFlowApplicationSteps.Any(ps =>
                        ps.WorkFlowApplication == x.step.WorkFlowApplication
                        && ps.User == null
                        && x.step.Step > ps.Step
                    )

                    && !Entities.WorkFlowApplicationSteps.Any(cs =>
                        cs.WorkFlowApplication == x.step.WorkFlowApplication
                        && cs.User == user
                        && x.step.Step == cs.Step
                    )

                    && (
                        x.tag.Tag == null ||
                        Entities.UserLevels.Any(ul =>
                            ul.Company == company
                            && ul.User == user
                            && ul.Department == x.instance.Prototypes.Department
                            && (ul.PrototypeType == null || ul.PrototypeType == x.instance.Prototypes.PrototypeType)
                            && ul.PrototypeWorkFlowLevel == x.tag.Tag
                        )
                    )

                    && (
                        Entities.UserFunctions.Any(uf =>
                            uf.Company == company
                            && uf.User == user
                            && uf.FunctionGroup == "Instances"
                            && uf.Function == "Confirm"
                            && uf.Department == x.instance.Prototypes.Department
                            && (uf.PrototypeType == null || uf.PrototypeType == x.instance.Prototypes.PrototypeType)
                        )

                        || Entities.UserGroups
                            .Join(
                                Entities.GroupFunctions,
                                ug => new { ug.Company, ug.Group },
                                gf => new { gf.Company, gf.Group },
                                (ug, gf) => new { ug, gf }
                            )
                            .Any(z =>
                                z.ug.Company == company
                                && z.ug.User == user
                                && z.gf.FunctionGroup == "Instances"
                                && z.gf.Function == "Confirm"
                                && z.gf.Department == x.instance.Prototypes.Department
                                && (z.gf.PrototypeType == null || z.gf.PrototypeType == x.instance.Prototypes.PrototypeType)
                            )

                        || Entities.UserRoles
                            .Join(
                                Entities.RoleFunctions,
                                ur => new { ur.Company, ur.Role },
                                rf => new { rf.Company, rf.Role },
                                (ur, rf) => new { ur, rf }
                            )
                            .Any(z =>
                                z.ur.Company == company
                                && z.ur.User == user
                                && z.rf.FunctionGroup == "Instances"
                                && z.rf.Function == "Confirm"
                                && z.ur.Department == x.instance.Prototypes.Department
                                && (z.ur.PrototypeType == null || z.ur.PrototypeType == x.instance.Prototypes.PrototypeType)
                            )

                        || Entities.UserGroups
                            .Join(
                                Entities.GroupRoles,
                                ug => new { ug.Company, ug.Group },
                                gr => new { gr.Company, gr.Group },
                                (ug, gr) => new { ug, gr }
                            )
                            .Join(
                                Entities.RoleFunctions,
                                z => new { z.gr.Company, z.gr.Role },
                                rf => new { rf.Company, rf.Role },
                                (z, rf) => new { z.ug, z.gr, rf }
                            )
                            .Any(z =>
                                z.ug.Company == company
                                && z.ug.User == user
                                && z.rf.FunctionGroup == "Instances"
                                && z.rf.Function == "Confirm"
                                && z.gr.Department == x.instance.Prototypes.Department
                                && (z.gr.PrototypeType == null || z.gr.PrototypeType == x.instance.Prototypes.PrototypeType)
                            )
                    )
                );
            //var list = result.Select(x => x.instance.ID).Distinct().ToList();
            return result.Select(x => x.instance.ID).Distinct().LongCountAsync();
        }

        private Task<long> CountPrototypeVersionsToSign(long company, string user)
        {
            var result = Entities.WorkFlowApplications
                .Join(
                    Entities.Prototypes,
                    application => application.Prototype,
                    prototype => prototype.ID,
                    (application, prototype) => new { application, prototype }
                )
                .Join(
                    Entities.WorkFlowApplicationSteps,
                    x => x.application.ID,
                    step => step.WorkFlowApplication,
                    (x, step) => new { x.prototype, x.application, step }
                )
                .Join(
                    Entities.WorkFlowApplicationStepTags,
                    x => x.step.ID,
                    tag => tag.WorkFlowApplicationStep,
                    (x, tag) => new { x.prototype, x.application, x.step, tag }
                )
                .Where(x =>
                    !x.application.Approved.HasValue

                    && x.step.User == null

                    && !Entities.WorkFlowApplicationSteps.Any(ps =>
                        ps.WorkFlowApplication == x.step.WorkFlowApplication
                        && ps.User == null
                        && x.step.Step > ps.Step
                    )

                    && !Entities.WorkFlowApplicationSteps.Any(cs =>
                        cs.WorkFlowApplication == x.step.WorkFlowApplication
                        && cs.User == user
                        && x.step.Step == cs.Step
                    )

                    && (
                        x.tag.Tag == null ||
                        Entities.UserLevels.Any(ul =>
                            ul.Company == company
                            && ul.User == user
                            && ul.Department == x.prototype.Department
                            && (ul.PrototypeType == null || ul.PrototypeType == x.prototype.PrototypeType)
                            && ul.PrototypeWorkFlowLevel == x.tag.Tag
                        )
                    )

                    && (
                        Entities.UserFunctions.Any(uf =>
                            uf.Company == company
                            && uf.User == user
                            && uf.FunctionGroup == "Prototypes"
                            && uf.Function == "Approve"
                            && uf.Department == x.prototype.Department
                            && (uf.PrototypeType == null || uf.PrototypeType == x.prototype.PrototypeType)
                        )

                        || Entities.UserGroups
                            .Join(
                                Entities.GroupFunctions,
                                ug => new { ug.Company, ug.Group },
                                gf => new { gf.Company, gf.Group },
                                (ug, gf) => new { ug, gf }
                            )
                            .Any(z =>
                                z.ug.Company == company
                                && z.ug.User == user
                                && z.gf.FunctionGroup == "Prototypes"
                                && z.gf.Function == "Approve"
                                && z.gf.Department == x.prototype.Department
                                && (z.gf.PrototypeType == null || z.gf.PrototypeType == x.prototype.PrototypeType)
                            )

                        || Entities.UserRoles
                            .Join(
                                Entities.RoleFunctions,
                                ur => new { ur.Company, ur.Role },
                                rf => new { rf.Company, rf.Role },
                                (ur, rf) => new { ur, rf }
                            )
                            .Any(z =>
                                z.ur.Company == company
                                && z.ur.User == user
                                && z.rf.FunctionGroup == "Prototypes"
                                && z.rf.Function == "Approve"
                                && z.ur.Department == x.prototype.Department
                                && (z.ur.PrototypeType == null || z.ur.PrototypeType == x.prototype.PrototypeType)
                            )

                        || Entities.UserGroups
                            .Join(
                                Entities.GroupRoles,
                                ug => new { ug.Company, ug.Group },
                                gr => new { gr.Company, gr.Group },
                                (ug, gr) => new { ug, gr }
                            )
                            .Join(
                                Entities.RoleFunctions,
                                z => new { z.gr.Company, z.gr.Role },
                                rf => new { rf.Company, rf.Role },
                                (z, rf) => new { z.ug, z.gr, rf }
                            )
                            .Any(z =>
                                z.ug.Company == company
                                && z.ug.User == user
                                && z.rf.FunctionGroup == "Prototypes"
                                && z.rf.Function == "Approve"
                                && z.gr.Department == x.prototype.Department
                                && (z.gr.PrototypeType == null || z.gr.PrototypeType == x.prototype.PrototypeType)
                            )
                    )
                );
            //var list = result.Select(x => x.prototype.ID).Distinct().ToList();
            return result.Select(x => x.prototype.ID).Distinct().LongCountAsync();
        }

        public async Task<NotificationsState> GetNotificationsAsync(long company, string user, NotificationsState currentState, Func<string, string, long?, long?, string, bool> hasPermission /*make the procedure faster if the authentication token is available*/)
        {
            var reportViewPermission = hasPermission == null || hasPermission("Reports", "View", null, null, null);
            var reportEditPermission = hasPermission == null || hasPermission("Reports", "Edit", null, null, null);
            var instancePermission = hasPermission == null || hasPermission("Instances", "Confirm", null, null, null);
            var prototypePermission = hasPermission == null || hasPermission("Prototypes", "Approve", null, null, null);
            var state = new NotificationsState
            {
                CompletedReportCount = reportViewPermission ? (currentState.CompletedReportCount ?? await Entities.ReportRuns.Where(r => r.Company == company && r.User == user && r.CompletedOn.HasValue).CountAsync()) : 0,
                EnqueuedReportCount = reportViewPermission ? (currentState.EnqueuedReportCount ?? await Entities.ReportRuns.Where(r => r.Company == company && r.User == user && !r.CompletedOn.HasValue).CountAsync()) : 0,
                ReportsToSignCount = reportEditPermission ? (currentState.ReportsToSignCount ?? await CountReportsToSign(company, user)) : 0,
                InstancesToSignCount = instancePermission ? (currentState.InstancesToSignCount ?? await CountInstancesToSign(company, user)) : 0,
                PrototypeVersionsToSignCount = prototypePermission ? (currentState.PrototypeVersionsToSignCount ?? await CountPrototypeVersionsToSign(company, user)) : 0
            };
            return state;
        }

    }
}
