using System.Collections.Generic;
using System.Threading.Tasks;
using CheckListManager.Common.Data;
using CheckListManager.Common.Core.Web.Security;
using System.Linq;
using CheckListManager.AccessControl.Data.Models;

namespace CheckListManager.AccessControl.Data
{
	public interface IUserRepository : IRepository
	{
		Task<Users> GetAsync(long companyId, string code);
        Task<Users> AddAsync(DefaultIdentity identity, Users user);
        Task<Users> EditAsync(DefaultIdentity identity, Users user);
        Task SetPINAsync(long companyId, string code, byte[] pin);
        Task DeleteAsync(long companyId, string code);

		Task<List<ExportRow>> GetUsersForExportAsync(long companyId, string code, string description, string corporateID);
		Task<List<Users>> GetUsersAsync(long companyId, string code, string description, string corporateID, int skip, int take);
		Task<int> CountUsersAsync(long companyId, string code, string description, string corporateID);
		Task UserLoginCompleteAsync(Users user);
		Task UserPasswordFailedAsync(Users user);
		Task UserPINFailedAsync(Users user);
		Task<List<Users>> GetUsersToDisableAsync(long company);
		Task<List<Users>> GetUsersDisableWarningAsync(long company);
		Task<List<Users>> GetUsersWithExpiredPin(long company);
	}
}
