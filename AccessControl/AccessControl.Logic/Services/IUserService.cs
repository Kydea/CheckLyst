using System;
using System.Threading.Tasks;
using CheckListManager.AccessControl.Logic.Authentication;
using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Data;

namespace CheckListManager.AccessControl.Logic.Services
{
	public interface IUserService : IService
	{
		Task<Users> AuthenticateAsync(string code, string password);

		Task<IdentityToken> GetTokenAsync(string code);
		Task CreateSessionAsync(long company, string code, string user, string ticket, DateTimeOffset expiresOn);
	}
}
