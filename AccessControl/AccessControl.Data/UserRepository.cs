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
			=> Entities.Users.Include(u => u.Companies).FirstOrDefaultAsync(u => u.Company == companyId && u.Code == code);

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
            var existingUser = await GetAsync(identity.CompanyId, user.Code);
            if (existingUser == null) throw new DatabaseException($"User {user.Code} not found");
            existingUser.Description = user.Description;
            existingUser.Email = user.Email;
            if (user.Password != null) existingUser.Password = user.Password;
            existingUser.Enabled = user.Enabled;
            existingUser.AuthenticationCode = user.AuthenticationCode;
            existingUser.ModifiedOn = DateTimeOffset.UtcNow;
            existingUser.ModifiedBy = identity.Code;
            Entities.UserFunctions.RemoveRange(existingUser.UserFunctions);
            existingUser.UserFunctions = user.UserFunctions;
            await SaveChangesAsync();
            return existingUser;
        }

        public async Task SetPINAsync(long companyId, string code, byte[] pin)
        {
            await Call<int>("\"Security\".\"SetUserPIN\"",
                new Parameter("_company", companyId),
                new Parameter("_user", code),
                new Parameter("_pin", pin));
        }

        public async Task DeleteAsync(long companyId, string code)
        {
            var user = await GetAsync(companyId, code);
            if (user == null) throw new DatabaseException($"User {code} not found");

            Entities.Users.Remove(user);
            await SaveChangesAsync();
        }


        private IQueryable<Users> Get(long companyId, string code, string description)
        {
	        return Entities.Users
		        .Where(
			        u => u.Company == companyId &&
						(code == string.Empty || u.Code != null && u.Code.ToUpper().Contains(code.ToUpper())) &&
						(description == string.Empty || u.Description != null && u.Description.ToUpper().Contains(description.ToUpper()))
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

		public async Task UserLoginCompleteAsync(Users user)
		{
			user.NumberOfAttempts = 0;
			user.LastLoginOn = DateTimeOffset.UtcNow;

			await SaveChangesAsync();
		}

		public async Task UserLoginFailedAsync(Users user)
		{
			if (user != null)
			{
				++user.NumberOfAttempts;
				user.LastLoginFailedOn = DateTimeOffset.UtcNow;

				await SaveChangesAsync();
			}
		}

		public Task<List<Users>> GetUsersToDisableAsync()
		{
			return Entities.Users
				.Include(u => u.Companies)
				.Where(
					x =>
						x.Enabled
						&& x.LastLoginOn < DbFunctions.AddDays(DateTime.UtcNow, -x.Companies.MaximumUserInactivityDays)
				).ToListAsync();
		}

		public Task<List<Users>> GetUsersDisableWarningAsync()
		{
			return Entities.Users
				.Include(u => u.Companies)
				.Where(
					x =>
						x.Enabled
						&& x.LastLoginOn < DbFunctions.AddDays(DateTime.UtcNow, -x.Companies.WarningUserInactivityDays)
				).ToListAsync();
		}

		public Task<List<Users>> GetUsersWithExpiredPin()
		{
			return Entities.Users
				.Include(u => u.Companies)
				.Where(
					x =>
						x.PIN != null
						&& x.PINChangeDate < DbFunctions.AddDays(DateTime.UtcNow, -x.Companies.PINDaysLife)
				).ToListAsync();
		}
	}
}