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

		public async Task CreateAsync(UserSessions session)
		{
			Entities.UserSessions.Add(session);
			await Entities.SaveChangesAsync();
		}
	}
}