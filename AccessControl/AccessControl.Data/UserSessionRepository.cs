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

		public Task<UserSessions> GetAsync(string code)
			=> Entities.UserSessions.FirstOrDefaultAsync(s => s.Code == code);

		public async Task CreateAsync(long company, string code, string user, string ticket, DateTimeOffset expiresOn)
		{
			var session = new UserSessions
			{
				Company = company,
				Code = code,
				User = user,
				Ticket = ticket,
				CreatedOn = DateTimeOffset.UtcNow,
				ExpiresOn = expiresOn
			};

			Entities.UserSessions.Add(session);

			await Entities.SaveChangesAsync();
		}
	}
}