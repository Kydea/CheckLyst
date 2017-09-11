using System.Linq;
using System.Threading.Tasks;
using CheckListManager.AccessControl.Data;
using CheckListManager.Common.Data;

namespace CheckListManager.AccessControl.Tests.Mocks
{
	public class UserRepositoryMock : RepositoryMock<Users>, IUserRepository
	{
		public Users Get(string code)
		{
			return Data.FirstOrDefault(u => u.Code == code);
		}

		public async Task<Users> GetAsync(string code)
		{
			return await Task.FromResult(Get(code));
		}
	}
}
