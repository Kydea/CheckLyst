using CheckListManager.Common.Core.Cache;
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

        public Task<List<Departments>> GetDepartmentsAsync(long company)
            => Entities.Departments.Where(d => d.Company == company).ToListAsync();

        public Task<List<PrototypeTypes>> GetPrototypeTypesAsync(long company, long department)
            => Entities.PrototypeTypes.Where(pt => pt.Company == company && pt.Department == department).ToListAsync();

        public Task<List<Prototypes>> GetPrototypesAsync(long company, long prototypeType)
            => Entities.Prototypes.Where(p => p.Company == company && p.PrototypeType == prototypeType).ToListAsync();

        public Task<List<ListItems.PrototypeVersion>> GetPrototypeVersionsAsync(long company, long prototype)
            => Entities.PrototypeVersions.Where(pv => pv.Prototypes.Company == company && pv.Prototype == prototype).Select(pv => new ListItems.PrototypeVersion { Version = pv.Version, VersionState = pv.VersionState }).ToListAsync();

        public Task<List<string>> GetPrototypeVersionTagsAsync(long company, long prototype, int? version)
            => Entities.PrototypeVersionTags.Where(pv => pv.PrototypeVersions.Prototypes.Company == company && pv.Prototype == prototype && (version == null || pv.Version == version)).Select(pv => pv.Code).ToListAsync();

        public Task<List<string>> GetVersionStatesAsync()
            => _memoryCache.GetOrAddAsync("VersionStates", key => Entities.VersionStates.Select(s => s.Code).ToListAsync(), "CommonRepository");

        public Task<List<string>> GetInstanceStatesAsync()
            => _memoryCache.GetOrAddAsync("InstanceStates", key => Entities.InstanceStates.Select(s => s.Code).ToListAsync(), "CommonRepository");
    }
}
