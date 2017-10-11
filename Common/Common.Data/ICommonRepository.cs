using System.Collections.Generic;
using System.Threading.Tasks;

namespace CheckListManager.Common.Data
{
    public interface ICommonRepository : IRepository
    {
        Task<List<Departments>> GetDepartmentsAsync(long company);
        Task<List<PrototypeTypes>> GetPrototypeTypesAsync(long department);
        Task<List<string>> GetVersionStatesAsync();
    }
}