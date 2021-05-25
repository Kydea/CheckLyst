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

        Task<UserResponse> LoadUserAsync(DefaultIdentity identity, UserRequest record);
        Task<UserResponse> AddUserAsync(DefaultIdentity identity, UserRequest request);
        Task<UserResponse> EditUserAsync(DefaultIdentity identity, UserRequest record);
        Task<UserResponse> DeleteUserAsync(DefaultIdentity identity, UserRequest record);

		Task CheckInactiveUsers(long company, CancellationToken cancellationToken = default(CancellationToken));
		Task CheckUnchangedPins(long company, CancellationToken cancellationToken = default(CancellationToken));
	}
}
