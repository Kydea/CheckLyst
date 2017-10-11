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
            => _memoryCache.GetOrAddAsync("DepartmentsOfCompany" + company, key => Entities.Departments.Where(d => d.Company == company).ToListAsync(), "CommonRepository");

        public Task<List<PrototypeTypes>> GetPrototypeTypesAsync(long department)
            => _memoryCache.GetOrAddAsync("PrototypeTypesOfDepartment" + department, key => Entities.PrototypeTypes.Where(d => d.Department == department).ToListAsync(), "CommonRepository");

        public Task<List<string>> GetVersionStatesAsync()
            => _memoryCache.GetOrAddAsync("VersionStates", key => Entities.VersionStates.Select(d => d.Code).ToListAsync(), "CommonRepository");
    }
}
