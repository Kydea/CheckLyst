using System.Collections.Generic;
using System.Threading.Tasks;

namespace CheckListManager.Common.Data
{
    public interface ICommonRepository : IRepository
    {
        Task<List<ListItems.Company>> GetCompaniesAsync();
        Task<List<Departments>> GetDepartmentsAsync(long company);
        Task<List<PrototypeTypes>> GetPrototypeTypesAsync(long company, long department);
        Task<List<Prototypes>> GetPrototypesAsync(long company, long prototypeType);
        Task<List<ListItems.PrototypeVersion>> GetPrototypeVersionsAsync(long company, long prototype);
        Task<List<string>> GetPrototypeVersionTagsAsync(long company, long prototype, int? Version);
        Task<List<string>> GetVersionStatesAsync();
        Task<List<string>> GetInstanceStatesAsync();
    }
}