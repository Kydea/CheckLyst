using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CheckListManager.AccessControl.Logic.Authentication;
using CheckListManager.AccessControl.Logic.Messages.Users;
using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.AccessControl.Data.Models;
using CheckListManager.AccessControl.Logic.Messages.UserSettings;

namespace CheckListManager.AccessControl.Logic.Services
{
	public interface IUserService : IService
	{
		Task<Users> AuthenticateAsync(long? companyId, string code, string password);
        Task SetPINAsync(long? companyId, string code, string password, string pin);
        Task AuthenticateWithPINAsync(long? companyId, string code, string pin);

        Task CreateSessionAsync(long company, string code, string user, string ticket, DateTimeOffset expiresOn);
		Task<IdentityToken> GetTokenAsync(string code);
		Task UpdateTokenAsync(IdentityToken token, DateTimeOffset expiresOn);
		Task LogoutAsync(DefaultIdentity identity, string token);

		Task<List<ExportRow>> GetFunctionsAsync(long companyId, UsersRequest request);
		Task<PagingResponse<List<UsersResponse>>> GetUsersAsync(long companyId, PagingRequest<UsersRequest> request);
        Task<UserResponse> BindAsync(UsersRequest data);

        Task<UserResponse> LoadUserAsync(DefaultIdentity identity, UserRequest record);
        Task<UserResponse> AddUserAsync(DefaultIdentity identity, UserRequest request);
        Task<UserResponse> EditUserAsync(DefaultIdentity identity, UserRequest record);
        Task<UserResponse> DeleteUserAsync(DefaultIdentity identity, UserRequest record);
        Task SetTimeZoneAsync(DefaultIdentity identity, string value);

        Task CheckInactiveUsers(long company, CancellationToken cancellationToken = default);
		Task CheckUnchangedPins(long company, CancellationToken cancellationToken = default);
        Task<List<Tuple<string, string>>> GetConflictsAsync(IDefaultIdentity identity, List<string> groups);
        
        Task<string> LoadSettingsAsync(DefaultIdentity identity, UserSettingsRequest data);
        Task SaveSettingsAsync(DefaultIdentity identity, UserSettingsRequest data);

        Task<Message<List<SummaryRecord>>> SummaryAsync(DefaultIdentity identity, UsersRequest data);
    }
}
