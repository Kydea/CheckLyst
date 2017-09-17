using System.Data.Entity;
using System.Threading.Tasks;
using CheckListManager.Common.Data;

namespace CheckListManager.AccessControl.Data
{
	public class UserRepository : Repository, IUserRepository
	{
		// ReSharper disable once MemberCanBeProtected.Global
		public UserRepository(RelationalDatabaseEntities entities) : base(entities)
		{			
		}

		public Task<Users> GetAsync(long companyId, string code)
			=> Entities.Users.FirstOrDefaultAsync(u => u.Company == companyId && u.Code == code);
	}
}