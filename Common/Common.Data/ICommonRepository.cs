using CheckListManager.Common.Core.Web.Security;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CheckListManager.Common.Data
{
    public interface ICommonRepository : IRepository
    {
        Task<Companies> GetCompanyAsync(long id);
        Task<List<ListItems.Company>> GetCompaniesAsync();
        Task<List<Departments>> GetDepartmentsAsync(IDefaultIdentity identity, string functionGroup, string function);
        Task<List<PrototypeTypes>> GetPrototypeTypesAsync(IDefaultIdentity identity, long department, string functionGroup, string function);
        Task<List<Departments>> GetDepartmentsAsync(long companyId);
        Task<List<PrototypeTypes>> GetPrototypeTypesAsync(long companyId);
        Task<List<Prototypes>> GetPrototypesAsync(IDefaultIdentity identity, long prototypeType, string functionGroup, string function);
        Task<List<ListItems.PrototypeVersion>> GetPrototypeVersionsAsync(IDefaultIdentity identity, long prototype, string functionGroup, string function);
        Task<List<string>> GetPrototypeVersionTagsAsync(IDefaultIdentity identity, long prototype, int? version, string functionGroup, string function);
        Task<List<string>> GetVersionStatesAsync();
        Task<List<string>> GetInstanceStatesAsync();
        Users GetUser(long CompanyID, string Code);
        Task<List<Users>> GetInstanceAppointedUsersAsync(long CompanyID, string FunctionGroup, string Function, long Department, long PrototypeType, IEnumerable<string> Tags);
        Task<List<Users>> GetPrototypeAppointedUsersAsync(long CompanyID, string FunctionGroup, string Function, long Department, long PrototypeType, IEnumerable<string> Tags);
        Task<FTPServers> GetPrototypesFTPServerAsync(long CompanyID, long PrototypeType);
        Task<FTPServers> GetInstancesFTPServerAsync(long CompanyID, long PrototypeType);
    }
}