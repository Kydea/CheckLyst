using System.Threading.Tasks;
using CheckListManager.Common.Core.Services;

namespace CheckListManager.Common.Logic.Authentication
{
	public interface IAuthenticationService : IService
	{
		Task AuthenticateAsync(long? companyId, string code, string password);

		Task AuthenticateWithPINAsync(long? companyId, string token, string pin);
    }
}
