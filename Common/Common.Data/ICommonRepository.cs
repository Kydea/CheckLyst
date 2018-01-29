using CheckListManager.Common.Core.Web.Security;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CheckListManager.Common.Data
{
    public interface ICommonRepository : IRepository
    {
        Task<Companies> GetCompanyAsync(long id);
        Task<List<ListItems.Company>> GetCompaniesAsync();
        Task<List<Departments>> GetDepartmentsAsync(IDefaultIdentity identity, string functionGroup);
        Task<List<PrototypeTypes>> GetPrototypeTypesAsync(IDefaultIdentity identity, long department, string functionGroup);
        Task<List<Prototypes>> GetPrototypesAsync(IDefaultIdentity identity, long prototypeType, string functionGroup);
        Task<List<ListItems.PrototypeVersion>> GetPrototypeVersionsAsync(IDefaultIdentity identity, long prototype, string functionGroup);
        Task<List<string>> GetPrototypeVersionTagsAsync(IDefaultIdentity identity, long prototype, int? version, string functionGroup);
        Task<List<string>> GetVersionStatesAsync();
        Task<List<string>> GetInstanceStatesAsync();
        Users GetUser(long CompanyID, string Code);
        List<Users> GetAppointedUsers(long CompanyID, string FunctionGroup, string Function, long Department, long PrototypeType, IEnumerable<string> Tags);
    }
}