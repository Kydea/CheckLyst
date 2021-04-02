using CheckListManager.Common.Core.Cache;
using CheckListManager.Common.Core.Web.Security;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace CheckListManager.Common.Data
{
    public class CommonRepository : Repository, ICommonRepository
    {
        private readonly IMemoryCache _memoryCache;

        public CommonRepository(RelationalDatabaseEntities entities, IMemoryCache memoryCache) : base(entities)
        {
            _memoryCache = memoryCache;
        }

        public Task<Companies> GetCompanyAsync(long id)
            => Entities.Companies.SingleAsync(c => c.ID == id);

        public Task<List<ListItems.Company>> GetCompaniesAsync()
            => _memoryCache.GetOrAddAsync("Companies", key =>
            Entities.Companies.Where(c => c.ID > 1)
            .OrderBy(c => c.Name).ThenBy(c => c.ID)
            .Select(c => new ListItems.Company
            {
                ID = c.ID,
                Name = c.Name,
                InactivityTimeoutSeconds = c.InactivityTimeoutSeconds
            }).ToListAsync(), "CommonRepository");

        public Task<List<Departments>> GetDepartmentsAsync(IDefaultIdentity identity, string functionGroup, string function)
        {
            IQueryable<Departments> Departments = Entities.Departments.Where(d => d.Company == identity.CompanyId);
            if (functionGroup != null)
                Departments = Departments.Where(d => d.UserFunctions.Any(uf =>
                    uf.Company == identity.CompanyId
                    && uf.User == identity.Code
                    && uf.FunctionGroup == functionGroup
                    && uf.Function == function
                ));
            return Departments.ToListAsync();
        }

        public Task<List<PrototypeTypes>> GetPrototypeTypesAsync(IDefaultIdentity identity, long department, string functionGroup, string function)
        {
            IQueryable<PrototypeTypes> PrototypeTypes = Entities.PrototypeTypes.Where(pt => pt.Company == identity.CompanyId && pt.Department == department);
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
            return PrototypeTypes.ToListAsync();
        }

        public async Task<List<Departments>> GetDepartmentsAsync(long companyId)
        {
            return await Entities.Departments.Where(d => d.Company == companyId).ToListAsync();
        }

        public async Task<List<PrototypeTypes>> GetPrototypeTypesAsync(long companyId)
        {
            return await Entities.PrototypeTypes.Where(pt => pt.Company == companyId).ToListAsync();
        }

        public Task<List<Prototypes>> GetPrototypesAsync(IDefaultIdentity identity, long prototypeType, string functionGroup, string function)
        {
            IQueryable<Prototypes> Prototypes = Entities.Prototypes.Where(p => p.Company == identity.CompanyId && p.PrototypeType == prototypeType);
            if (functionGroup != null)
                Prototypes = Prototypes.Where(p => p.PrototypeTypes.UserFunctions.Any(uf =>
                    uf.Company == identity.CompanyId
                    && uf.User == identity.Code
                    && uf.FunctionGroup == functionGroup
                    && uf.Function == function
                ) || p.Departments.UserFunctions.Any(uf =>
                    uf.Company == identity.CompanyId
                    && uf.User == identity.Code
                    && uf.FunctionGroup == functionGroup
                    && uf.Function == function
                    && uf.PrototypeType == null
                ));
            if (functionGroup == "Instances")
                Prototypes = Prototypes.Where(p => p.PrototypeVersions.Any(pv => pv.VersionState == VersionState.Draft || pv.VersionState == VersionState.InSimulation));
            return Prototypes.ToListAsync();
        }

        public Task<List<ListItems.PrototypeVersion>> GetPrototypeVersionsAsync(IDefaultIdentity identity, long prototype, string functionGroup, string function)
        {
            IQueryable<PrototypeVersions> PrototypeVersion = Entities.PrototypeVersions.Where(pv => pv.Prototypes.Company == identity.CompanyId && pv.Prototype == prototype);
            if (functionGroup != null)
                PrototypeVersion = PrototypeVersion.Where(pv => pv.Prototypes.PrototypeTypes.UserFunctions.Any(uf =>
                    uf.Company == identity.CompanyId
                    && uf.User == identity.Code
                    && uf.FunctionGroup == functionGroup
                    && uf.Function == function
                ) || pv.Prototypes.Departments.UserFunctions.Any(uf =>
                    uf.Company == identity.CompanyId
                    && uf.User == identity.Code
                    && uf.FunctionGroup == functionGroup
                    && uf.Function == function
                    && uf.PrototypeType == null
                ));

            return PrototypeVersion.Select(pv => new ListItems.PrototypeVersion { Version = pv.Version, VersionState = pv.VersionState }).ToListAsync();
        }

        public Task<List<string>> GetPrototypeVersionTagsAsync(IDefaultIdentity identity, long prototype, int? version, string functionGroup, string function)
        {
            IQueryable<PrototypeVersionTags> PrototypeVersionTags = Entities.PrototypeVersionTags.Where(pv => pv.PrototypeVersions.Prototypes.Company == identity.CompanyId && pv.Prototype == prototype && (version == null || pv.Version == version));
            if (functionGroup != null)
                PrototypeVersionTags = PrototypeVersionTags.Where(pvt => pvt.PrototypeVersions.Prototypes.PrototypeTypes.UserFunctions.Any(uf =>
                    uf.Company == identity.CompanyId
                    && uf.User == identity.Code
                    && uf.FunctionGroup == functionGroup
                    && uf.Function == function
                ) || pvt.PrototypeVersions.Prototypes.Departments.UserFunctions.Any(uf =>
                    uf.Company == identity.CompanyId
                    && uf.User == identity.Code
                    && uf.FunctionGroup == functionGroup
                    && uf.Function == function
                    && uf.PrototypeType == null
               ));
            return PrototypeVersionTags.Select(pv => pv.Code).Distinct().ToListAsync();
        }

        public Task<List<string>> GetVersionStatesAsync()
            => _memoryCache.GetOrAddAsync("VersionStates", key => Entities.VersionStates.Select(s => s.Code).ToListAsync(), "CommonRepository");

        public Task<List<string>> GetInstanceStatesAsync()
            => _memoryCache.GetOrAddAsync("InstanceStates", key => Entities.InstanceStates.Select(s => s.Code).ToListAsync(), "CommonRepository");


        public Users GetUser(long CompanyID, string Code)
        {
            return Entities.Users.SingleOrDefault(u => u.Company == CompanyID && u.Code == Code);
        }

        private IQueryable<Users> GetAppointedUsersQuery(long CompanyID, string FunctionGroup, string Function, long Department, long PrototypeType, IEnumerable<string> Tags)
        {
            return Entities.UserFunctions.Where(uf =>
                uf.Company == CompanyID
                && uf.FunctionGroup == FunctionGroup
                && uf.Function == Function
                && uf.Department == Department
                && (uf.PrototypeType == null || uf.PrototypeType == PrototypeType)
                && (Tags.Count() == 0 || Tags.Contains(uf.Tag))
            ).Select(uf => uf.Users);
        }

        public List<Users> GetAppointedUsers(long CompanyID, string FunctionGroup, string Function, long Department, long PrototypeType, IEnumerable<string> Tags)
        {
            return GetAppointedUsersQuery(CompanyID, FunctionGroup, Function, Department, PrototypeType, Tags).ToList();
        }

        public async Task<List<Users>> GetAppointedUsersAsync(long CompanyID, string FunctionGroup, string Function, long Department, long PrototypeType, IEnumerable<string> Tags)
        {
            return await GetAppointedUsersQuery(CompanyID, FunctionGroup, Function, Department, PrototypeType, Tags).ToListAsync();
        }

        public async Task<FTPServers> GetPrototypesFTPServerAsync(long CompanyID, long PrototypeType)
        {
            var PrototypeTypeRow = await Entities.PrototypeTypes.SingleAsync(pt => pt.Company == CompanyID && pt.ID == PrototypeType);
            return
                !PrototypeTypeRow.PrototypesFTPServerEnabled ? null : await Entities.FTPServers.SingleOrDefaultAsync(ftps =>
                    ftps.Company == CompanyID && (!PrototypeTypeRow.PrototypesFTPServer.HasValue && ftps.PrototypesDefault || ftps.ID == PrototypeTypeRow.PrototypesFTPServer));
        }

        public async Task<FTPServers> GetInstancesFTPServerAsync(long CompanyID, long PrototypeType)
        {
            var PrototypeTypeRow = await Entities.PrototypeTypes.SingleAsync(pt => pt.Company == CompanyID && pt.ID == PrototypeType);
            return
                !PrototypeTypeRow.InstancesFTPServerEnabled ? null : await Entities.FTPServers.SingleOrDefaultAsync(ftps =>
                    ftps.Company == CompanyID && (!PrototypeTypeRow.InstancesFTPServer.HasValue && ftps.InstancesDefault || ftps.ID == PrototypeTypeRow.InstancesFTPServer));
        }
    }
}
