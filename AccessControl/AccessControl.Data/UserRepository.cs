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

		public Task AddAsync(Users user)
		{
			Entities.Users.Add(user);
			return SaveChangesAsync();
		}

        private IQueryable<Users> Get(long companyId, string Keyword)
        {
            Keyword = Keyword?.ToUpper() ?? "";
            return Entities.Users
                .Where(
                    u => u.Company == companyId && 
                    (
                        u.Code.ToUpper().Contains(Keyword)
                        || u.Description != null && u.Description.ToUpper().Contains(Keyword)
                        || u.Email != null && u.Email.ToUpper().Contains(Keyword)
                    )
                );
        }

		public Task<List<Users>> GetUsersAsync(long companyId, string keyword, int skip, int take)
			=> Get(companyId, keyword)
				.OrderBy(u => u.Code)
				.Skip(skip)
				.Take(take)
				.ToListAsync();

		public Task<int> CountUsersAsync(long companyId, string keyword)
			=> Get(companyId, keyword)
				.CountAsync();
	}
}