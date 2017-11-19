using System.Collections.Generic;
using System.Threading.Tasks;
using CheckListManager.Common.Data;

namespace CheckListManager.AccessControl.Data
{
	public interface IUserRepository : IRepository
	{
		Task<Users> GetAsync(long companyId, string code);
		Task AddAsync(Users user);

		Task<List<Users>> GetUsersAsync(long companyId, string code, string description, int skip, int take);
		Task<int> CountUsersAsync(long companyId, string code, string description);
	}
}
