using System.Collections.Generic;
using System.Threading.Tasks;
using CheckListManager.Common.Data;
using CheckListManager.Common.Core.Web.Security;
using System.Linq;

namespace CheckListManager.AccessControl.Data
{
	public interface IUserRepository : IRepository
	{
		Task<Users> GetAsync(long companyId, string code);
        Task<Users> AddAsync(DefaultIdentity identity, Users user);
        Task<Users> EditAsync(DefaultIdentity identity, Users user);
        Task SetPINAsync(long companyId, string code, byte[] pin);
        Task DeleteAsync(long companyId, string code);

		Task<List<UserFunctions>> GetFunctionsAsync(long companyId, string code, string description);
		Task<List<Users>> GetUsersAsync(long companyId, string code, string description, int skip, int take);
		Task<int> CountUsersAsync(long companyId, string code, string description);
		Task UserLoginCompleteAsync(Users user);
		Task UserPasswordFailedAsync(Users user);
		Task UserPINFailedAsync(Users user);
		Task<List<Users>> GetUsersToDisableAsync();
		Task<List<Users>> GetUsersDisableWarningAsync();
		Task<List<Users>> GetUsersWithExpiredPin();
	}
}
