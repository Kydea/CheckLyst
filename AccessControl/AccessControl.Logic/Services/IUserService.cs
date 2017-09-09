using System.Threading.Tasks;
using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Data;

namespace CheckListManager.AccessControl.Logic.Services
{
	public interface IUserService : IService
	{
		Task<Users> AuthenticateAsync(string code, string password);
	}
}
