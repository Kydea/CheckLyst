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

        public Task<Companies> GetCompanyAsync(long id) => 
            Entities.Companies.SingleAsync(c => c.ID == id);

        public Task<List<ListItems.Company>> GetCompaniesAsync() => 
            _memoryCache.GetOrAddAsync("Companies", key =>
                Entities.Companies
                    .OrderBy(c => c.Name).ThenBy(c => c.ID)
                    .Select(c => new ListItems.Company
                    {
                        ID = c.ID,
                        Name = c.Name,
                    }).ToListAsync(), "CommonRepository");


        private const string DEPARTMENTS_QUERY =
            "select d.* from \"Infrastructure\".\"Departments\" d " +
            "where " +
                "d.\"Company\" = :company " +
                "and d.\"ID\" in (select ug.\"Department\" from \"Security\".\"GetUserGrants\"(:company, :user, :functionGroup, :function) ug)" +
            "order by d.\"Name\"";
        public async Task<List<Departments>> GetDepartmentsAsync(IDefaultIdentity identity, string functionGroup, string function)
        {
            var Departments = await Read<Departments>(DEPARTMENTS_QUERY, new { company = identity.CompanyId, user = identity.Code, functionGroup, function });
            return Departments.ToList();
        }

        private const string PROTOTYPE_TYPES_QUERY =
            "select pt.*, d.\"Name\" \"DepartmentName\" " +
            "from " +
                "\"CheckLists\".\"PrototypeTypes\" pt " +
                "inner join \"Infrastructure\".\"Departments\" d on pt.\"Department\" = d.\"ID\" " +
            "where " +
                "pt.\"Company\" = :company " +
                "and (:department = -1 or \"Department\" = :department) " +
                "and exists (" +
                    "select 1 from \"Security\".\"GetUserGrants\"(:company, :user, :functionGroup, :function) g " +
                    "where g.\"Department\" = pt.\"Department\" and (g.\"PrototypeType\" is null or g.\"PrototypeType\" = pt.\"ID\")" +
                ")" +
            "order by pt.\"Name\"";
        public async Task<List<PrototypeTypes>> GetPrototypeTypesAsync(IDefaultIdentity identity, long department, string functionGroup, string function)
        {
            var PrototypeTypes = await Read<PrototypeTypes>(PROTOTYPE_TYPES_QUERY, new { company = identity.CompanyId, user = identity.Code, functionGroup, function, department});
            return PrototypeTypes.ToList();
        }

        private const string PROTOTYPES_QUERY =
            "select p.* " +
            "from \"CheckLists\".\"Prototypes\" p " +
            "where " +
                "p.\"Company\" = :company " +
                "and (:department = -1 or \"Department\" = :department) " +
                "and (:prototypetype = -1 or \"PrototypeType\" = :prototypeType) " +
                "and exists (" +
                    "select 1 from \"Security\".\"GetUserGrants\"(:company, :user, :functionGroup, :function) g " +
                    "where g.\"Department\" = p.\"Department\" and (g.\"PrototypeType\" is null or g.\"PrototypeType\" = p.\"PrototypeType\")" +
                ")" +
            "order by p.\"Name\"";
        public async Task<List<Prototypes>> GetPrototypesAsync(IDefaultIdentity identity, long department, long prototypeType, string functionGroup, string function)
        {
            var Prototypes = await Read<Prototypes>(PROTOTYPES_QUERY, new { company = identity.CompanyId, user = identity.Code, functionGroup, function, department, prototypeType });
            return Prototypes.ToList();
        }

        private const string PROTOTYPE_VERSIONS_QUERY =
            "select pv.* " +
            "from " +
                "\"CheckLists\".\"PrototypeVersions\" pv " +
                "inner join \"CheckLists\".\"Prototypes\" p on pv.\"Prototype\" = p.\"ID\" " +
            "where " +
                "p.\"Company\" = :company " +
                "and pv.\"Prototype\" = :prototype " +
                "and exists (" +
                    "select 1 from \"Security\".\"GetUserGrants\"(:company, :user, :functionGroup, :function) g " +
                    "where g.\"Department\" = p.\"Department\" and (g.\"PrototypeType\" is null or g.\"PrototypeType\" = p.\"PrototypeType\")" +
                ")" +
            "order by pv.\"Version\"";
        public async Task<List<ListItems.PrototypeVersion>> GetPrototypeVersionsAsync(IDefaultIdentity identity, long prototype, string functionGroup, string function)
        {
            var PrototypeVersions = await Read<ListItems.PrototypeVersion>(PROTOTYPE_VERSIONS_QUERY, new { company = identity.CompanyId, user = identity.Code, prototype, functionGroup, function });
            return PrototypeVersions.ToList();
        }

        private const string PROTOTYPE_VERSION_TAGS_QUERY =
            "select distinct pvt.\"Code\" " +
            "from " +
                "\"CheckLists\".\"PrototypeVersionTags\" pvt " +
                "inner join \"CheckLists\".\"PrototypeVersions\" pv on pvt.\"Prototype\" = pv.\"Prototype\" and pvt.\"Version\" = pv.\"Version\" " +
                "inner join \"CheckLists\".\"Prototypes\" p on pv.\"Prototype\" = p.\"ID\" " +
            "where " +
                "p.\"Company\" = :company " +
                "and pv.\"Prototype\" = :prototype " +
                "and (:version = -1 or pv.\"Version\" = :version) " +
                "and exists (" +
                    "select 1 from \"Security\".\"GetUserGrants\"(:company, :user, :functionGroup, :function) g " +
                    "where g.\"Department\" = p.\"Department\" and (g.\"PrototypeType\" is null or g.\"PrototypeType\" = p.\"PrototypeType\")" +
                ")" +
            "order by pvt.\"Code\"";
        public async Task<List<string>> GetPrototypeVersionTagsAsync(IDefaultIdentity identity, long prototype, int? version, string functionGroup, string function)
        {
            var PrototypeVersions = await Read<string>(PROTOTYPE_VERSION_TAGS_QUERY, new { company = identity.CompanyId, user = identity.Code, prototype, version = version ?? -1, functionGroup, function });
            return PrototypeVersions.ToList();
        }

        public async Task<List<string>> GetAppointedUsersAsync(long companyID, string functionGroup, string function, long department, long prototypeType, string levelName, IEnumerable<string> levels)
        {
            string query =
                "select u.\"Code\" " +
                "from " +
                    "\"Security\".\"GetGrantedUsers\"(:company, :department, :prototypetype, :functiongroup, :function) u " +
                    " and exists (" +
                        "select 1 " +
                        "from \"Security\".\"UserLevels\" ul " +
                        "where " +
                            "ul.\"Company\" = u.\"Company\" " +
                            "and ul.\"Code\" = u.\"Code\" " +
                            "and (ul.\"Department\" is null || ul.\"Department\" = :department) " +
                            "and (ul.\"PrototypeType\" is null || ul.\"PrototypeType\" = :prototypeType) " +
                            (
                                levels.Any() ?
                                    string.Empty : 
                                    $"and ul.\"{levelName}\" in ({string.Join(", ", levels.Select(l => l.Replace("\'", "\'\'")))}) "
                            ) +
                    ") " +
                "order by u.\"Code\"";

            var users = await Read<string>(query, new { company = companyID, functionGroup, function });
            return users.ToList();
        }

        public async Task<List<Departments>> GetDepartmentsAsync(long companyId) =>
            await Entities.Departments.Where(d => d.Company == companyId).ToListAsync();

        public async Task<List<PrototypeTypes>> GetPrototypeTypesAsync(long companyId, long department) =>
            await Entities.PrototypeTypes.Where(pt => pt.Company == companyId && pt.Department == department).ToListAsync();

        public Task<List<string>> GetVersionStatesAsync() => 
            _memoryCache.GetOrAddAsync("VersionStates", key => Entities.VersionStates.Select(s => s.Code).ToListAsync(), "CommonRepository");

        public Task<List<string>> GetInstanceStatesAsync() => 
            _memoryCache.GetOrAddAsync("InstanceStates", key => Entities.InstanceStates.Select(s => s.Code).ToListAsync(), "CommonRepository");
        
        public Users GetUser(long CompanyID, string Code) =>
            Entities.Users.SingleOrDefault(u => u.Company == CompanyID && u.Code == Code);

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
