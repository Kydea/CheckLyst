using System;
using System.Threading.Tasks;
using CheckListManager.Common.Data;

namespace CheckListManager.AccessControl.Data
{
	public interface IUserSessionRepository : IRepository
	{
		Task CreateAsync(UserSessions session);
		Task<UserSessions> GetAsync(string code);
		Task SetExpiresOnAsync(string code, DateTimeOffset expiresOn);
	}
}
