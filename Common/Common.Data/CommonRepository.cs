using CheckListManager.Common.Core.Cache;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Data
{
    public class CommonRepository : Repository, ICommonRepository
    {
        IMemoryCache _memoryCache;
        public CommonRepository(RelationalDatabaseEntities entities, IMemoryCache memoryCache) : base(entities)
        {
            _memoryCache = memoryCache;
        }

        public Task<List<Departments>> GetDepartmentsAsync(long Company)
            => _memoryCache.GetOrAddAsync("DepartmentsOfCompany" + Company, key => Entities.Departments.Where(d => d.Company == Company).ToListAsync(), "CommonRepository");

        public Task<List<PrototypeTypes>> GetPrototypeTypesAsync(long Department)
            => _memoryCache.GetOrAddAsync("PrototypeTypesOfDepartment" + Department, key => Entities.PrototypeTypes.Where(d => d.Department == Department).ToListAsync(), "CommonRepository");

        public Task<List<string>> GetVersionStatesAsync()
            => _memoryCache.GetOrAddAsync("VersionStates", key => Entities.VersionStates.Select(d => d.Code).ToListAsync(), "CommonRepository");
    }
}
