using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CheckListManager.Common.Data
{
    public interface ICommonRepository : IRepository
    {
        Task<List<Departments>> GetDepartmentsAsync(long Company);
        Task<List<PrototypeTypes>> GetPrototypeTypesAsync(long Department);
        Task<List<string>> GetVersionStatesAsync();
    }
}