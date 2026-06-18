using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using CheckListManager.Common.Data;
using CheckListManager.Common.Core.Web.Security;
using System;
using CheckListManager.Common.Logic.Services;
using CheckListManager.AccessControl.Data.Models;
using CheckListManager.Common.Core.Exceptions;

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
                "u.\"Code\", u.\"Description\", u.\"Email\", u.\"CorporateID\", u.\"Enabled\", u.\"LastLoginOn\",  u.\"NumberOfPasswordAttempts\", u.\"ReportWorkFlowLevel\", " +
                "g.\"Group\", g.\"Department\", d.\"Name\" \"DepartmentName\", g.\"PrototypeType\", pt.\"Name\" \"PrototypeTypeName\", " +
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
                "and (:code = '' or u.\"Code\" ilike '%' || :code || '%') " +
                "and (:description = '' or u.\"Description\" ilike '%' || :description || '%') " +
                "and (:corporateID = '' or u.\"CorporateID\" ilike '%' || :corporateID || '%') " +
                "and (:group = '' or exists (select 1 from \"Security\".\"UserGroups\" ug where ug.\"Company\" = u.\"Company\" and ug.\"User\" = u.\"Code\" and ug.\"Group\" ilike '%' || :group || '%')) " +
                "and (:role = '' or exists (select 1 from \"Security\".\"UserRoles\" ur where ur.\"Company\" = u.\"Company\" and ur.\"User\" = u.\"Code\" and ur.\"Role\" ilike '%' || :role || '%')) " +
                "and (:failedLogins is null or u.\"NumberOfPasswordAttempts\" >= :failedLogins) " +
                "and ( " +
                    ":inactivityDays is null " +
                    "or (u.\"LastLoginOn\" is null and date_part('day', now() - u.\"CreatedOn\") >= :inactivityDays) " +
                    "or (u.\"LastLoginOn\" is not null and date_part('day', now() - u.\"LastLoginOn\") >= :inactivityDays) " +
                ") " +
                "and (:enabled = 'All' or :enabled = 'Enabled' and u.\"Enabled\" or :enabled = 'Disabled' and not u.\"Enabled\") " +
            "order by u.\"Code\", g.\"Department\", g.\"PrototypeType\", g.\"Role\", g.\"FunctionGroup\", g.\"Function\"";

        private const string SUMMARY_QUERY =
            "select " +
                "c.\"ID\" \"CompanyID\", max(c.\"Name\") \"CompanyDescription\", " +
                "count(*) \"TotalUsers\", " +
                "sum(case when not :from is null and to_char(u.\"LastLoginOn\", 'yyyy-mm-dd') >= :from then 1 else 0 end) \"ActiveUsers\" " +
            "from " +
                "\"Security\".\"Users\" u " +
                "inner join \"Infrastructure\".\"Companies\" c on u.\"Company\" = c.\"ID\" " +
            "where " +
                "u.\"Enabled\" " +
            "group by c.\"ID\" " +
            "order by c.\"ID\"";

        public async Task<List<ExportRow>> GetUsersForExportAsync(long company, string code, string description, string corporateID, string group, string role, int? failedLogins, int? inactivityDays, string enabled)
        {
            var List = await Read<ExportRow>(EXPORT_QUERY, new
            {
                company,
                code = code?.Replace("_", "\\_").Replace("%", "\\%") ?? string.Empty,
                description = description?.Replace("_", "\\_").Replace("%", "\\%") ?? string.Empty,
                corporateID = corporateID?.Replace("_", "\\_").Replace("%", "\\%") ?? string.Empty,
                group = group?.Replace("_", "\\_").Replace("%", "\\%") ?? string.Empty,
                role = role?.Replace("_", "\\_").Replace("%", "\\%") ?? string.Empty,
                failedLogins,
                inactivityDays,
                enabled
            });
            return List.ToList();
        }

        public async Task<List<ExportRow>> GetRolesForExportAsync(long company, string code)
        {
            var roles = GetRolesAsync(company, code);

            var List = await roles.SelectMany(r => r.RoleFunctions.Select(rf => new ExportRow
            {
                Code = r.Code,
                FunctionGroup = rf.FunctionGroup,
                Function = rf.Function
            })).OrderBy(er => er.Code).ThenBy(er => er.FunctionGroup).ThenBy(er => er.Function).ToListAsync();
            return List;
        }


        public async Task<List<ExportRow>> GetGroupsForExportAsync(long company, string code, string description, string user, string role)
        {
            var groups = GetGroupsAsync(company, code, description, user, role);

            var List = groups.SelectMany(r => r.GroupFunctions.Select(gf => new ExportRow
            {
                Code = r.Code,
                Description = r.Description,
                Department = gf.Department,
                DepartmentName = gf.Departments == null ? null : gf.Departments.Name,
                PrototypeType = gf.PrototypeType,
                PrototypeTypeName = gf.PrototypeTypes == null ? null : gf.PrototypeTypes.Name,
                Role = null,
                FunctionGroup = gf.FunctionGroup,
                Function = gf.Function
            }));
            List = List.Union(
                groups.SelectMany(r => r.GroupRoles.Select(gr => new ExportRow
                {
                    Code = r.Code,
                    Description = r.Description,
                    Department = gr.Department,
                    DepartmentName = gr.Departments == null ? null : gr.Departments.Name,
                    PrototypeType = gr.PrototypeType,
                    PrototypeTypeName = gr.PrototypeTypes == null ? null : gr.PrototypeTypes.Name,
                    Role = gr.Role,
                    FunctionGroup = null,
                    Function = null
                })).OrderBy(er => er.Code).ThenBy(er => er.Department).ThenBy(er => er.PrototypeType).ThenBy(er => er.FunctionGroup).ThenBy(er => er.Function)
            );
            return await List.OrderBy(er => er.Code).ThenBy(er => er.Department).ThenBy(er => er.PrototypeType).ThenBy(er => er.FunctionGroup).ThenBy(er => er.Function).ToListAsync();
        }

        public async Task UserLoginCompleteAsync(Users user)
        {
            user.NumberOfPasswordAttempts = 0;
            user.LastLoginOn = DateTimeOffset.UtcNow;

            await SaveChangesAsync();
        }

        public async Task<bool> UserPasswordFailedAsync(Users user)
        {
            bool forgiven = true;
            if (user != null)
            {
                ++user.NumberOfPasswordAttempts;
                user.LastLoginFailedOn = DateTimeOffset.UtcNow;

                int MaximumNumberOfPasswordFailure = await _settingsService.MaximumNumberOfPasswordFailureAsync(user.Company);

                if (MaximumNumberOfPasswordFailure > 0 && user.NumberOfPasswordAttempts >= MaximumNumberOfPasswordFailure && user.Enabled)
                    forgiven = user.Enabled = false;

                await SaveChangesAsync();
            }
            return forgiven;
        }

        public async Task<bool> UserPINFailedAsync(Users user)
        {
            bool forgiven = true;
            if (user != null)
            {
                ++user.NumberOfPINAttempts;

                int MaximumNumberOfPINFailure = await _settingsService.MaximumNumberOfPINFailureAsync(user.Company);

                if (MaximumNumberOfPINFailure > 0 && user.NumberOfPINAttempts >= MaximumNumberOfPINFailure && user.PIN != null)
                {
                    forgiven = false;
                    user.PIN = null;
                }

                await SaveChangesAsync();
            }
            return forgiven;
        }

        public async Task<List<Users>> GetUsersToDisableAsync(long company)
        {
            var Days = await _settingsService.MaximumUserInactivityDaysAsync(company);
            if (Days == 0) return new List<Users>(0);
            var Threshold = DateTime.UtcNow.AddDays(-Days);
            var Users = await Entities.Users
                .Include(u => u.Companies)
                .Where(x => x.Company == company && x.Enabled && (x.LastLoginOn < Threshold || x.LastLoginOn == null && x.CreatedOn < Threshold)).ToListAsync();

            return Users;
        }

        public async Task<List<Users>> GetUsersDisableWarningAsync(long company)
        {
            var Days = await _settingsService.WarningUserInactivityDaysAsync(company);
            if (Days == 0) return new List<Users>(0);
            var Threshold = DateTime.UtcNow.AddDays(-Days);
            return await Entities.Users
                .Include(u => u.Companies)
                .Where(x => x.Company == company && x.Enabled && (x.LastLoginOn < Threshold || x.LastLoginOn == null && x.CreatedOn < Threshold)).ToListAsync();
        }

        public async Task<List<Users>> GetUsersWithExpiredPin(long company)
        {
            var Threshold = DateTime.UtcNow.AddDays(-await _settingsService.PINDaysLifeAsync(company));
            return await Entities.Users.Where(x => x.Company == company && x.PIN != null && x.PINChangeDate < Threshold).ToListAsync();
        }

        private IQueryable<Users> GetAsync(long company, string code, string description, string corporateID, string group, string role, int? failedLogins, int? inactivityDays, string enabled)
            =>
            Entities.Users
                .Where(u =>
                    u.Company == company
                    && (code == string.Empty || u.Code != null && u.Code.ToUpper().Contains(code.ToUpper()))
                    && (description == string.Empty || u.Description != null && u.Description.ToUpper().Contains(description.ToUpper()))
                    && (corporateID == string.Empty || u.CorporateID != null && u.CorporateID.ToUpper().Contains(corporateID.ToUpper()))
                    && (group == string.Empty || u.UserGroups.Any(ug => ug.Group.ToUpper().Contains(group.ToUpper())))
                    && (role == string.Empty || u.UserRoles.Any(ug => ug.Role.ToUpper().Contains(role.ToUpper())))
                    && (failedLogins == null || u.NumberOfPasswordAttempts >= failedLogins.Value)
                    && (
                        inactivityDays == null
                        || (u.LastLoginOn == null && DbFunctions.DiffDays(u.CreatedOn, DateTimeOffset.UtcNow) >= inactivityDays.Value)
                        || (u.LastLoginOn != null && DbFunctions.DiffDays(u.LastLoginOn, DateTimeOffset.UtcNow) >= inactivityDays.Value)
                    )
                    && (enabled == "All" || enabled == "Enabled" && u.Enabled || enabled == "Disabled" && !u.Enabled)
                );

        public Task<List<Users>> GetUsersAsync(long companyId, string code, string description, string corporateID, string group, string role, int? failedLogins, int? inactivityDays, string enabled, int skip, int take)
            => GetAsync(companyId, code, description, corporateID, group, role, failedLogins, inactivityDays, enabled)
                .OrderBy(u => u.Code)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

        public Task<int> CountUsersAsync(long company, string code, string description, string corporateID, string group, string role, int? failedLogins, int? inactivityDays, string enabled)
            => GetAsync(company, code, description, corporateID, group, role, failedLogins, inactivityDays, enabled).CountAsync();

        public async Task<Users> AddAsync(DefaultIdentity identity, Users user)
        {
            user.Company = identity.CompanyId;
            user.CreatedOn = DateTimeOffset.UtcNow;
            user.CreatedBy = identity.Code;
            Entities.Users.Add(user);
            await SaveChangesAsync();
            return user;
        }

        public async Task<Users> EditAsync(DefaultIdentity identity, Users user, bool licenceExpired)
        {
            var existingUser = await GetAsync(identity.CompanyId, user.Code) ?? throw new DatabaseException($"User {user.Code} not found");

            if (licenceExpired && !existingUser.Enabled && user.Enabled)
                throw new DatabaseException($"The maximum number of active users allowed by the license has been reached");

            existingUser.Description = user.Description;
            existingUser.Email = user.Email;
            existingUser.CorporateID = user.CorporateID;
            if (user.Password != null) existingUser.Password = user.Password;
            existingUser.Enabled = user.Enabled;
            if (user.Enabled) existingUser.NumberOfPasswordAttempts = 0;
            existingUser.AuthenticationCode = user.AuthenticationCode;
            existingUser.ReportWorkFlowLevel = user.ReportWorkFlowLevel;
            existingUser.TimeZone = user.TimeZone;
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
            var user = await GetAsync(company, code) ?? throw new DatabaseException($"User {code} not found");
            Entities.Users.Remove(user);
            await SaveChangesAsync();
        }

        public Task<long> CountUsersAsync(long companyId, bool? enabled) =>
            Entities.Users.LongCountAsync(u => u.Company == companyId && (enabled == true && u.Enabled || enabled == false && !u.Enabled || !enabled.HasValue));

        public async Task SetTimeZoneAsync(long companyId, string code, string value)
        {
            var user = await GetAsync(companyId, code) ?? throw new DatabaseException($"User {code} not found");
            user.TimeZone = value == "-" ? null : value;
            await SaveChangesAsync();
        }

        public Task<Roles> GetRoleAsync(long companyId, string code) =>
            Entities.Roles.SingleOrDefaultAsync(u => u.Company == companyId && u.Code.ToUpper() == code.ToUpper());

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
            var existingRole = await GetRoleAsync(identity.CompanyId, role.Code) ?? throw new DatabaseException($"Role {role.Code} not found");
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
            var role = await GetRoleAsync(company, code) ?? throw new DatabaseException($"Role {code} not found");

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

        private IQueryable<Groups> GetGroupsAsync(long company, string code, string description, string user, string role) =>
            Entities.Groups
                .Where(
                    u => u.Company == company
                        && (code == string.Empty || u.Code != null && u.Code.ToUpper().Contains(code.ToUpper()))
                        && (description == string.Empty || u.Description != null && u.Description.ToUpper().Contains(description.ToUpper()))
                        && (user == string.Empty || u.UserGroups.Any(ug => ug.User.ToUpper().Contains(user.ToUpper())))
                        && (role == string.Empty || u.GroupRoles.Any(gr => gr.Role.ToUpper().Contains(role.ToUpper())))
                );

        public Task<List<Groups>> GetGroupsAsync(long companyId, string code, string description, string user, string role, int skip, int take)
            => GetGroupsAsync(companyId, code, description, user, role)
                .OrderBy(u => u.Code)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

        public Task<int> CountGroupsAsync(long company, string code, string description, string user, string role)
            => GetGroupsAsync(company, code, description, user, role).CountAsync();

        public async Task<Groups> AddGroupAsync(DefaultIdentity identity, Groups group)
        {
            group.Company = identity.CompanyId;
            group.CreatedOn = DateTimeOffset.UtcNow;
            group.CreatedBy = identity.Code;
            Entities.Groups.Add(group);
            await SaveChangesAsync();
            return group;
        }

        public async Task<Groups> EditGroupAsync(DefaultIdentity identity, Groups group, List<string> existingConflicts)
        {
            var existingGroup = await GetGroupAsync(identity.CompanyId, group.Code) ?? throw new DatabaseException($"Group {group.Code} not found");
            existingGroup.Description = group.Description;
            existingGroup.ModifiedOn = DateTimeOffset.UtcNow;
            existingGroup.ModifiedBy = identity.Code;
            Entities.GroupRoles.RemoveRange(existingGroup.GroupRoles);
            Entities.GroupFunctions.RemoveRange(existingGroup.GroupFunctions);
            existingConflicts.AddRange(existingGroup.GroupConflicts.Select(c => c.Group2));
            existingConflicts.AddRange(existingGroup.GroupConflicts1.Select(c => c.Group1));
            Entities.GroupConflicts.RemoveRange(existingGroup.GroupConflicts);
            Entities.GroupConflicts.RemoveRange(existingGroup.GroupConflicts1);
            existingGroup.GroupRoles = group.GroupRoles;
            existingGroup.GroupFunctions = group.GroupFunctions;
            existingGroup.GroupConflicts = group.GroupConflicts;
            await SaveChangesAsync();
            return existingGroup;
        }

        public async Task DeleteGroupAsync(long company, string code)
        {
            var user = await GetGroupAsync(company, code) ?? throw new DatabaseException($"Group {code} not found");

            Entities.Groups.Remove(user);
            await SaveChangesAsync();
        }

        public async Task<List<GroupConflicts>> GetConflicts(long company, List<string> groups) =>
            groups == null || !groups.Any() ?
                new List<GroupConflicts>(0) :
                await Entities.GroupConflicts.Where(c => c.Company == company && groups.Contains(c.Group1) && groups.Contains(c.Group2)).ToListAsync();

        public Task<string> LoadSettingsAsync(long company, string user, string setting, string code) =>
            Entities.UserSettingValues.Where(x => x.Company == company && x.User == user && x.Setting == setting && x.Code == code).Select(x => x.Value).SingleOrDefaultAsync();

        public async Task SaveSettingsAsync(long company, string user, string setting, string code, string value)
        {
            var record = await Entities.UserSettingValues.SingleOrDefaultAsync(sv => sv.Company == company && sv.User == user && sv.Setting == setting && sv.Code == code);

            if (value == null && record != null)
                Entities.UserSettingValues.Remove(record);
            else if (value != null && record == null)
            {
                record = new UserSettingValues
                {
                    Company = company,
                    User = user,
                    Setting = setting,
                    Code = code,
                    Value = value,
                    CreatedOn = DateTimeOffset.UtcNow
                };
                Entities.UserSettingValues.Add(record);
            }
            else if (value != null && record != null)
            {
                record.Value = value;
                record.ModifiedOn = DateTimeOffset.UtcNow;
            }
            await SaveChangesAsync();
        }

        public async Task<List<SummaryRecord>> SummaryAsync(DefaultIdentity identity, DateTimeOffset? from)
        {
            var Filters = new
            {
                from = from?.ToString("yyyy-MM-dd") ?? string.Empty
            };

            var List = await Read<SummaryRecord>(SUMMARY_QUERY, Filters);
            return List.ToList();
        }
    } 

}