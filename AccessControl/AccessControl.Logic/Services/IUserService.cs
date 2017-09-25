using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CheckListManager.AccessControl.Logic.Authentication;
using CheckListManager.AccessControl.Logic.Messages.Users;
using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Messages;

namespace CheckListManager.AccessControl.Logic.Services
{
	public interface IUserService : IService
	{
		Task<Users> AuthenticateAsync(long? companyId, string code, string password);

		Task CreateSessionAsync(long company, string code, string user, string ticket, DateTimeOffset expiresOn);
		Task<IdentityToken> GetTokenAsync(string code);
		Task LogoutAsync(string code);

		Task<PagingResponse<List<UsersResponse>>> GetUsersAsync(long companyId, PagingRequest<UsersRequest> request);

		Task<AddUserResponse> AddUserAsync(long companyId, AddUserRequest request);
	}
}
