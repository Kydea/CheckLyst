using System.Collections.Generic;
using System.Threading.Tasks;
using CheckListManager.Common.Data;
using CheckListManager.Common.Core.Web.Security;
using System.Linq;
using CheckListManager.AccessControl.Data.Models;
using System;

namespace CheckListManager.AccessControl.Data
{
	public interface IUserRepository : IRepository
	{
        Task SetPINAsync(long companyId, string code, byte[] pin);

		Task<List<ExportRow>> GetUsersForExportAsync(long companyId, string code, string description, string corporateID, string group, string role, int? failedLogins, int? inactivityDays, string enabled);
		Task<List<ExportRow>> GetRolesForExportAsync(long companyId, string code);
        Task<List<ExportRow>> GetGroupsForExportAsync(long companyId, string code, string description, string user, string role);

		Task UserLoginCompleteAsync(Users user);
		Task<bool> UserPasswordFailedAsync(Users user);
		Task<bool> UserPINFailedAsync(Users user);
		Task<List<Users>> GetUsersToDisableAsync(long company);
		Task<List<Users>> GetUsersDisableWarningAsync(long company);
		Task<List<Users>> GetUsersWithExpiredPin(long company);

        Task<Users> GetAsync(long companyId, string code);
        Task<List<Users>> GetUsersAsync(long companyId, string code, string description, string corporateID, string group, string role, int? failedLogins, int? inactivityDays, string enabled, int skip, int take);
        Task<int> CountUsersAsync(long companyId, string code, string description, string corporateID, string group, string role, int? failedLogins, int? inactivityDays, string enabled);
        Task<Users> AddAsync(DefaultIdentity identity, Users user);
        Task<Users> EditAsync(DefaultIdentity identity, Users user);
        Task DeleteAsync(long companyId, string code);
        Task<long> CountUsersAsync(bool? enabled);

        Task<Roles> GetRoleAsync(long companyId, string code);
        Task<List<Roles>> GetRolesAsync(long companyId, string code, int skip, int take);
        Task<int> CountRolesAsync(long companyId, string code);
        Task<Roles> AddRoleAsync(DefaultIdentity identity, Roles role);
        Task<Roles> EditRoleAsync(DefaultIdentity identity, Roles role);
        Task DeleteRoleAsync(long companyId, string code);
        Task<List<Roles>> GetRolesAsync(long companyId);

        Task<Groups> GetGroupAsync(long companyId, string code);
        Task<List<Groups>> GetGroupsAsync(long companyId, string code, string description, string user, string role, int skip, int take);
        Task<int> CountGroupsAsync(long companyId, string code, string description, string user, string role);
        Task<Groups> AddGroupAsync(DefaultIdentity identity, Groups group);
        Task<Groups> EditGroupAsync(DefaultIdentity identity, Groups group, List<string> existingConflicts);
        Task DeleteGroupAsync(long companyId, string code);
        Task<List<GroupConflicts>> GetConflicts(long company, List<string> groups);
    }
}
