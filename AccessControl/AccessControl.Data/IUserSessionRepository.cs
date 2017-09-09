using System;
using System.Threading.Tasks;
using CheckListManager.Common.Data;

namespace CheckListManager.AccessControl.Data
{
	public interface IUserSessionRepository : IRepository
	{
		Task<UserSessions> GetAsync(string code);

		Task CreateAsync(long company, string code, string user, string ticket, DateTimeOffset expiresOn);
	}
}
