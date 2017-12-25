using System.Collections.Generic;
using System.Threading.Tasks;
using CheckListManager.Common.Data;
using CheckListManager.Common.Core.Web.Security;

namespace CheckListManager.AccessControl.Data
{
	public interface IUserRepository : IRepository
	{
		Task<Users> GetAsync(long companyId, string code);
        Task<Users> AddAsync(DefaultIdentity identity, Users user);
        Task<Users> EditAsync(DefaultIdentity identity, Users user);
        Task DeleteAsync(long companyId, string code);

		Task<List<Users>> GetUsersAsync(long companyId, string code, string description, int skip, int take);
		Task<int> CountUsersAsync(long companyId, string code, string description);
		Task UpdateLastLoginAsync(Users user);
		Task<List<Users>> GetUsersToDisableAsync();
		Task<List<Users>> GetUsersDisableWarningAsync();
		Task<List<Users>> GetUsersWithExpiredPin();
	}
}
