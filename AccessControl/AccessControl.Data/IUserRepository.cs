using System.Threading.Tasks;
using CheckListManager.Common.Data;

namespace CheckListManager.AccessControl.Data
{
	public interface IUserRepository : IRepository
	{
		Users Get(string code);
		Task<Users> GetAsync(string code);
	}
}
