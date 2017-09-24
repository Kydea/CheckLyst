using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
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

		public Task<List<Users>> GetUsersAsync(long companyId, string code, int skip, int take)
			=> Entities.Users
				.Where(u => u.Company == companyId && u.Code.StartsWith(code))
				.OrderBy(u => u.Code)
				.Skip(skip)
				.Take(take)
				.ToListAsync();

		public Task<int> CountUsersAsync(long companyId, string code)
			=> Entities.Users
				.Where(u => u.Company == companyId && u.Code.StartsWith(code))
				.CountAsync();
	}
}