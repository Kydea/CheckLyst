using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using CheckListManager.Common.Data;
using CheckListManager.Common.Core.Web.Security;
using System;
using CheckListManager.Common.Data.Exceptions;
using CheckListManager.Common.Logic.Services;

namespace CheckListManager.AccessControl.Data
{
    public class UserRepository : Repository, IUserRepository
    {
        private readonly ISettingService _settingsService;

        public UserRepository(RelationalDatabaseEntities entities, ISettingService settingsService) : base(entities)
        {
            _settingsService = settingsService;
        }

        public Task<Users> GetAsync(long companyId, string code)
            => Entities.Users.Include(u => u.Companies).FirstOrDefaultAsync(u => u.Company == companyId && u.Code.ToUpper() == code.ToUpper());

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
            existingUser.CorporateID = user.CorporateID;
            if (user.Password != null) existingUser.Password = user.Password;
            existingUser.Enabled = user.Enabled;
            if (user.Enabled) existingUser.NumberOfPasswordAttempts = 0;
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


        private IQueryable<Users> Get(long companyId, string code, string description, string corporateID)
        {
            return Entities.Users
                .Where(
                    u => u.Company == companyId &&
                        (code == string.Empty || u.Code != null && u.Code.ToUpper().Contains(code.ToUpper())) &&
                        (description == string.Empty || u.Description != null && u.Description.ToUpper().Contains(description.ToUpper())) &&
                        (corporateID == string.Empty || u.CorporateID != null && u.CorporateID.ToUpper().Contains(corporateID.ToUpper()))
                );
        }

        public async Task<List<UserFunctions>> GetFunctionsAsync(long companyId, string code, string description, string corporateID)
        {
            return await Entities.UserFunctions
                .Where(
                    f => f.Company == companyId &&
                        (code == string.Empty || f.User != null && f.User.ToUpper().Contains(code.ToUpper())) &&
                        (description == string.Empty || f.Users.Description != null && f.Users.Description.ToUpper().Contains(description.ToUpper())) &&
                        (corporateID == string.Empty || f.Users.CorporateID != null && f.Users.CorporateID.ToUpper().Contains(corporateID.ToUpper()))
                )
                .OrderBy(x => x.User).ThenBy(x => x.Department).ThenBy(x => x.PrototypeType).ThenBy(x => x.FunctionGroup).ThenBy(x => x.Function)
                .ToListAsync();
        }

        public Task<List<Users>> GetUsersAsync(long companyId, string code, string description, string corporateID, int skip, int take)
            => Get(companyId, code, description, corporateID)
                .OrderBy(u => u.Code)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

        public Task<int> CountUsersAsync(long companyId, string code, string description, string corporateID)
            => Get(companyId, code, description, corporateID).CountAsync();

        public async Task UserLoginCompleteAsync(Users user)
        {
            user.NumberOfPasswordAttempts = 0;
            user.LastLoginOn = DateTimeOffset.UtcNow;

            await SaveChangesAsync();
        }

        public async Task UserPasswordFailedAsync(Users user)
        {
            if (user != null)
            {
                ++user.NumberOfPasswordAttempts;
                user.LastLoginFailedOn = DateTimeOffset.UtcNow;

                int MaximumNumberOfPasswordFailure = await _settingsService.MaximumNumberOfPasswordFailureAsync(user.Company);

                if (MaximumNumberOfPasswordFailure > 0 && user.NumberOfPasswordAttempts >= MaximumNumberOfPasswordFailure)
                    user.Enabled = false;

                await SaveChangesAsync();
            }
        }

        public async Task UserPINFailedAsync(Users user)
        {
            if (user != null)
            {
                ++user.NumberOfPINAttempts;

                int MaximumNumberOfPINFailure = await _settingsService.MaximumNumberOfPINFailureAsync(user.Company);

                if (MaximumNumberOfPINFailure > 0 && user.NumberOfPINAttempts >= MaximumNumberOfPINFailure)
                    user.PIN = null;

                await SaveChangesAsync();
            }
        }

        public async Task<List<Users>> GetUsersToDisableAsync(long company)
		{
            var Threshold = DateTime.UtcNow.AddDays(-await _settingsService.MaximumUserInactivityDaysAsync(company));
            return await Entities.Users
				.Include(u => u.Companies)
				.Where(x => x.Enabled && x.LastLoginOn < Threshold).ToListAsync();
		}

		public async Task<List<Users>> GetUsersDisableWarningAsync(long company)
		{
            var Threshold = DateTime.UtcNow.AddDays(-await _settingsService.WarningUserInactivityDaysAsync(company));
            return await Entities.Users
				.Include(u => u.Companies)
				.Where(x => x.Enabled && x.LastLoginOn < Threshold).ToListAsync();
		}

		public async Task<List<Users>> GetUsersWithExpiredPin(long company)
		{
            var Threshold = DateTime.UtcNow.AddDays(-await _settingsService.PINDaysLifeAsync(company));
            return await Entities.Users.Where(x => x.PIN != null && x.PINChangeDate < Threshold).ToListAsync();
		}
	}
}