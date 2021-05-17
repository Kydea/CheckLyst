using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using CheckListManager.Common.Data;
using CheckListManager.Common.Core.Web.Security;
using System;
using CheckListManager.Common.Data.Exceptions;
using CheckListManager.Common.Logic.Services;
using CheckListManager.AccessControl.Data.Models;

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

        public async Task SetPINAsync(long companyId, string code, byte[] pin)
        {
            await Call<int>("\"Security\".\"SetUserPIN\"",
                new Parameter("_company", companyId),
                new Parameter("_user", code),
                new Parameter("_pin", pin));
        }

        private const string EXPORT_QUERY =
            "select " +
                "u.\"Code\", u.\"Description\", u.\"Email\", u.\"CorporateID\", u.\"Enabled\", u.\"LastLoginOn\", " +
                "g.\"Department\", d.\"Name\" \"DepartmentName\", g.\"PrototypeType\", pt.\"Name\" \"PrototypeTypeName\", " +
                "g.\"Role\", g.\"FunctionGroup\", g.\"Function\", (" +
                    "select \"SignatureLevel\" " +
                    "from \"Security\".\"UserLevels\" ul " +
                    "where " +
                        "ul.\"Company\" = u.\"Company\" " +
                        "and ul.\"User\" = u.\"Code\" " +
                        "and (ul.\"Department\" is null or ul.\"Department\" = g.\"Department\") " +
                        "and (ul.\"PrototypeType\" is null or ul.\"PrototypeType\" = g.\"PrototypeType\") " +
                        "and not \"SignatureLevel\" is null " +
                    "order by ul.\"PrototypeType\" " + //prototype type = null is considered only if the specific prototype type is not found
                    "limit 1" +
                ") \"SignatureLevel\", (" +
                    "select \"PrototypeWorkFlowLevel\" " +
                    "from \"Security\".\"UserLevels\" ul " +
                    "where " +
                        "ul.\"Company\" = u.\"Company\" " +
                        "and ul.\"User\" = u.\"Code\" " +
                        "and (ul.\"Department\" is null or ul.\"Department\" = g.\"Department\") " +
                        "and (ul.\"PrototypeType\" is null or ul.\"PrototypeType\" = g.\"PrototypeType\") " +
                        "and not \"PrototypeWorkFlowLevel\" is null " +
                    "order by ul.\"PrototypeType\" " + //prototype type = null is considered only if the specific prototype type is not found
                    "limit 1" +
                ") \"PrototypeWorkFlowLevel\", (" +
                    "select \"InstanceWorkFlowLevel\" " +
                    "from \"Security\".\"UserLevels\" ul " +
                    "where " +
                        "ul.\"Company\" = u.\"Company\" " +
                        "and ul.\"User\" = u.\"Code\" " +
                        "and (ul.\"Department\" is null or ul.\"Department\" = g.\"Department\") " +
                        "and (ul.\"PrototypeType\" is null or ul.\"PrototypeType\" = g.\"PrototypeType\") " +
                        "and not \"InstanceWorkFlowLevel\" is null " +
                    "order by ul.\"PrototypeType\" " + //prototype type = null is considered only if the specific prototype type is not found
                    "limit 1" +
                ") \"InstanceWorkFlowLevel\" " +
            "from " +
                "\"Security\".\"Users\" u " +
                "inner join lateral \"Security\".\"GetAllUserGrants\"(u.\"Company\", u.\"Code\") g on true " +
                "left join \"Infrastructure\".\"Departments\" d on g.\"Department\" = d.\"ID\" " +
                "left join \"CheckLists\".\"PrototypeTypes\" pt on g.\"PrototypeType\" = pt.\"ID\" " +
            "where " +
                "u.\"Company\" = :company " +
                "and (:code = '' or upper(u.\"Code\") like '%' || :code || '%') " +
                "and (:description = '' or upper(u.\"Description\") like '%' || :description || '%') " +
                "and (:corporateID = '' or upper(u.\"CorporateID\") like '%' || :corporateID || '%') " +
            "order by u.\"Code\", g.\"Department\", g.\"PrototypeType\", g.\"Role\", g.\"FunctionGroup\", g.\"Function\"";
        public async Task<List<ExportRow>> GetUsersForExportAsync(long company, string code, string description, string corporateID)
        {
            var List = await Read<ExportRow>(EXPORT_QUERY, new
            {
                company,
                code = code?.ToUpper() ?? string.Empty,
                description = description?.ToUpper() ?? string.Empty,
                corporateID = corporateID?.ToUpper() ?? string.Empty
            });
            return List.ToList();
        }

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
            var Days = await _settingsService.MaximumUserInactivityDaysAsync(company);
            if (Days == 0) return new List<Users>(0);
            var Threshold = DateTime.UtcNow.AddDays(-Days);
            var Users = await Entities.Users
                .Include(u => u.Companies)
                .Where(x => x.Enabled && (x.LastLoginOn < Threshold || x.LastLoginOn == null && x.CreatedOn < Threshold)).ToListAsync();

            foreach (var User in Users)
            {
                Logger.Error(e);
                User.ModifiedBy = $"{company}|{Days}|{Threshold.ToString("dd/MM/yyyy")}";
            }
            return Users;
        }

        public async Task<List<Users>> GetUsersDisableWarningAsync(long company)
        {
            var Days = await _settingsService.WarningUserInactivityDaysAsync(company);
            if (Days == 0) return new List<Users>(0);
            var Threshold = DateTime.UtcNow.AddDays(-Days);
            return await Entities.Users
                .Include(u => u.Companies)
                .Where(x => x.Enabled && (x.LastLoginOn < Threshold || x.LastLoginOn == null && x.CreatedOn < Threshold)).ToListAsync();
        }

        public async Task<List<Users>> GetUsersWithExpiredPin(long company)
        {
            var Threshold = DateTime.UtcNow.AddDays(-await _settingsService.PINDaysLifeAsync(company));
            return await Entities.Users.Where(x => x.PIN != null && x.PINChangeDate < Threshold).ToListAsync();
        }

        private IQueryable<Users> GetAsync(long company, string code, string description, string corporateID, string group) 
            =>
            Entities.Users
                .Where(
                    u => u.Company == company 
                        && (code == string.Empty || u.Code != null && u.Code.ToUpper().Contains(code.ToUpper())) 
                        && (description == string.Empty || u.Description != null && u.Description.ToUpper().Contains(description.ToUpper())) 
                        && (corporateID == string.Empty || u.CorporateID != null && u.CorporateID.ToUpper().Contains(corporateID.ToUpper()))
                        && (group == string.Empty || u.UserGroups.Any(ug => ug.Group.ToUpper().Contains(group.ToUpper())))
                );

        public Task<List<Users>> GetUsersAsync(long companyId, string code, string description, string corporateID, string group, int skip, int take)
            => GetAsync(companyId, code, description, corporateID, group)
                .OrderBy(u => u.Code)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

        public Task<int> CountUsersAsync(long company, string code, string description, string corporateID, string group)
            => GetAsync(company, code, description, corporateID, group).CountAsync();

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
            Entities.UserGroups.RemoveRange(existingUser.UserGroups);
            Entities.UserRoles.RemoveRange(existingUser.UserRoles);
            Entities.UserFunctions.RemoveRange(existingUser.UserFunctions);
            Entities.UserLevels.RemoveRange(existingUser.UserLevels);
            existingUser.UserGroups = user.UserGroups;
            existingUser.UserRoles = user.UserRoles;
            existingUser.UserFunctions = user.UserFunctions;
            existingUser.UserLevels = user.UserLevels;
            await SaveChangesAsync();
            return existingUser;
        }

        public async Task DeleteAsync(long company, string code)
        {
            var user = await GetAsync(company, code);
            if (user == null) throw new DatabaseException($"User {code} not found");

            Entities.Users.Remove(user);
            await SaveChangesAsync();
        }

        public Task<Roles> GetRoleAsync(long companyId, string code)
            => Entities.Roles.SingleOrDefaultAsync(u => u.Company == companyId && u.Code.ToUpper() == code.ToUpper());

        private IQueryable<Roles> GetRolesAsync(long company, string code)
        {
            return Entities.Roles
                .Where(
                    u => u.Company == company &&
                        (code == string.Empty || u.Code != null && u.Code.ToUpper().Contains(code.ToUpper()))
                );
        }

        public Task<List<Roles>> GetRolesAsync(long companyId, string code, int skip, int take)
            => GetRolesAsync(companyId, code)
                .OrderBy(u => u.Code)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

        public Task<int> CountRolesAsync(long company, string code)
            => GetRolesAsync(company, code).CountAsync();

        public async Task<Roles> AddRoleAsync(DefaultIdentity identity, Roles role)
        {
            role.Company = identity.CompanyId;
            role.CreatedOn = DateTimeOffset.UtcNow;
            role.CreatedBy = identity.Code;
            Entities.Roles.Add(role);
            await SaveChangesAsync();
            return role;
        }

        public async Task<Roles> EditRoleAsync(DefaultIdentity identity, Roles role)
        {
            var existingRole = await GetRoleAsync(identity.CompanyId, role.Code);
            if (existingRole == null) throw new DatabaseException($"Role {role.Code} not found");
            existingRole.FunctionGroup = role.FunctionGroup;
            existingRole.ModifiedOn = DateTimeOffset.UtcNow;
            existingRole.ModifiedBy = identity.Code;
            Entities.RoleFunctions.RemoveRange(existingRole.RoleFunctions);
            existingRole.RoleFunctions = role.RoleFunctions;
            await SaveChangesAsync();
            return existingRole;
        }

        public async Task DeleteRoleAsync(long company, string code)
        {
            var role = await GetRoleAsync(company, code);
            if (role == null) throw new DatabaseException($"Role {code} not found");

            Entities.Roles.Remove(role);
            await SaveChangesAsync();
        }

        public Task<List<Roles>> GetRolesAsync(long companyId)
            => GetRolesAsync(companyId, string.Empty)
                .Include(r => r.RoleFunctions)
                .OrderBy(r => r.Code)
                .ToListAsync();


        public Task<Groups> GetGroupAsync(long companyId, string code)
            => Entities.Groups.SingleOrDefaultAsync(u => u.Company == companyId && u.Code.ToUpper() == code.ToUpper());

        private IQueryable<Groups> GetGroupsAsync(long company, string code, string description, string user)
        {
            return Entities.Groups
                .Where(
                    u => u.Company == company 
                        && (code == string.Empty || u.Code != null && u.Code.ToUpper().Contains(code.ToUpper())) 
                        && (description == string.Empty || u.Description != null && u.Description.ToUpper().Contains(description.ToUpper()))
                        && (user == string.Empty || u.UserGroups.Any(ug => ug.User.ToUpper().Contains(user.ToUpper())))
                );
        }

        public Task<List<Groups>> GetGroupsAsync(long companyId, string code, string description, string user, int skip, int take)
            => GetGroupsAsync(companyId, code, description, user)
                .OrderBy(u => u.Code)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

        public Task<int> CountGroupsAsync(long company, string code, string description, string user)
            => GetGroupsAsync(company, code, description, user).CountAsync();

        public async Task<Groups> AddGroupAsync(DefaultIdentity identity, Groups group)
        {
            group.Company = identity.CompanyId;
            group.CreatedOn = DateTimeOffset.UtcNow;
            group.CreatedBy = identity.Code;
            Entities.Groups.Add(group);
            await SaveChangesAsync();
            return group;
        }

        public async Task<Groups> EditGroupAsync(DefaultIdentity identity, Groups group)
        {
            var existingGroup = await GetGroupAsync(identity.CompanyId, group.Code);
            if (existingGroup == null) throw new DatabaseException($"Group {group.Code} not found");
            existingGroup.Description = group.Description;
            existingGroup.ModifiedOn = DateTimeOffset.UtcNow;
            existingGroup.ModifiedBy = identity.Code;
            Entities.GroupRoles.RemoveRange(existingGroup.GroupRoles);
            Entities.GroupFunctions.RemoveRange(existingGroup.GroupFunctions);
            existingGroup.GroupRoles = group.GroupRoles;
            existingGroup.GroupFunctions = group.GroupFunctions;
            await SaveChangesAsync();
            return existingGroup;
        }

        public async Task DeleteGroupAsync(long company, string code)
        {
            var user = await GetGroupAsync(company, code);
            if (user == null) throw new DatabaseException($"Group {code} not found");

            Entities.Groups.Remove(user);
            await SaveChangesAsync();
        }

    }
}