using System;
using System.Data.Entity;
using System.Threading.Tasks;
using CheckListManager.Common.Data;

namespace CheckListManager.AccessControl.Data
{
	public class UserSessionRepository : Repository, IUserSessionRepository
	{
		public UserSessionRepository(RelationalDatabaseEntities entities) : base(entities)
		{
		}

		public Task CreateAsync(UserSessions session)
		{
			Entities.UserSessions.Add(session);
			return SaveChangesAsync();
		}

		public Task<UserSessions> GetAsync(string code)
			=> Entities.UserSessions.FirstOrDefaultAsync(s => s.Code == code);

		public async Task SetExpiresOnAsync(string code, DateTimeOffset expiresOn)
		{
			var session = await GetAsync(code);
			if (session != null && session.ExpiresOn >= DateTimeOffset.UtcNow)
			{
				session.ExpiresOn = expiresOn;
				await SaveChangesAsync();
			}
		}
	}
}