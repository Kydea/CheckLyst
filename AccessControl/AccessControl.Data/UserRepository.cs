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

        private IQueryable<Users> Get(long companyId, string code, string description)
        {
            return Entities.Users
                .Where(
                    u => u.Company == companyId && 
                    (
                        (code == string.Empty || u.Code != null && u.Code.ToUpper().Contains(code.ToUpper()))
                        && (description == string.Empty || u.Description != null && u.Description.ToUpper().Contains(description.ToUpper()))
                    )
                );
        }

		public Task<List<Users>> GetUsersAsync(long companyId, string code, string description, int skip, int take)
			=> Get(companyId, code, description)
				.OrderBy(u => u.Code)
				.Skip(skip)
				.Take(take)
				.ToListAsync();

		public Task<int> CountUsersAsync(long companyId, string code, string description)
			=> Get(companyId, code, description)
				.CountAsync();
	}
}