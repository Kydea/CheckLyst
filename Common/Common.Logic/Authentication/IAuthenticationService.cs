using System.Threading.Tasks;
using CheckListManager.Common.Core.Services;

namespace CheckListManager.Common.Logic.Authentication
{
	public interface IAuthenticationService : IService
	{
		Task AuthenticateAsync(string code, string password);

		Task AuthenticateWithPINAsync(string pin);
    }
}
