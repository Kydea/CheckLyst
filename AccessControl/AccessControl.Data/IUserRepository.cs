using System.Threading.Tasks;
using CheckListManager.Common.Data;

namespace CheckListManager.AccessControl.Data
{
	public interface IUserRepository : IRepository
	{
		Task<Users> GetAsync(string code);
	}
}
