using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using CheckListManager.Common.Data;
using CheckListManager.Common.Core.Web.Security;
using System;
using CheckListManager.Common.Data.Exceptions;

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

		public async Task<Users> AddAsync(DefaultIdentity identity, Users user)
		{
            user.Company = identity.CompanyId;
            user.CreatedOn = DateTimeOffset.UtcNow;
            user.CreatedBy = identity.Code;
            Entities.Users.Add(user);
			await SaveChangesAsync();
            return user;
        }

        public async Task<Users> EditAsync(DefaultIdentity identity, Users user)
        {
            var ExistingUser = await GetAsync(identity.CompanyId, user.Code);
            if (ExistingUser == null) throw new DatabaseException($"User {user.Code} not found");
            ExistingUser.Description = user.Description;
            ExistingUser.Email = user.Email;
            if (user.Password != null) ExistingUser.Password = user.Password;
            ExistingUser.Enabled = user.Enabled;
            ExistingUser.AuthenticationCode = user.AuthenticationCode;
            ExistingUser.ModifiedOn = DateTimeOffset.UtcNow;
            ExistingUser.ModifiedBy = identity.Code;
            Entities.UserFunctions.RemoveRange(ExistingUser.UserFunctions);
            ExistingUser.UserFunctions = user.UserFunctions;
            await SaveChangesAsync();
            return ExistingUser;
        }

        public async Task DeleteAsync(long companyId, string code)
        {
            var User = await GetAsync(companyId, code);
            if (User == null) throw new DatabaseException($"User {code} not found");

            Entities.Users.Remove(User);
            await SaveChangesAsync();
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