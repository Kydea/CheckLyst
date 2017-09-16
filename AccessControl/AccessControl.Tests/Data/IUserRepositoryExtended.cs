using System.Threading.Tasks;
using CheckListManager.AccessControl.Data;
using CheckListManager.Common.Data;

namespace CheckListManager.AccessControl.Tests.Data
{
	public interface IUserRepositoryExtended : IUserRepository
	{
		Task CreateTestUser(Users user);
	}
}
