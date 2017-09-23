using System.Collections.Generic;
using System.Threading.Tasks;
using CheckListManager.Common.Data;

namespace CheckListManager.AccessControl.Data
{
	public interface IUserRepository : IRepository
	{
		Task<Users> GetAsync(long companyId, string code);

		Task<List<Users>> GetUsersAsync(string code, int skip, int take);
		Task<int> CountUsersAsync(string code);
	}
}
