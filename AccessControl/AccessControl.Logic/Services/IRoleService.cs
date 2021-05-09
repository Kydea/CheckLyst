using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CheckListManager.AccessControl.Logic.Authentication;
using CheckListManager.AccessControl.Logic.Messages.Roles;
using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.AccessControl.Data.Models;

namespace CheckListManager.AccessControl.Logic.Services
{
	public interface IRoleService : IService
	{
		Task<PagingResponse<List<RolesResponse>>> GetRolesAsync(long companyId, PagingRequest<RolesRequest> request);

        Task<RoleResponse> LoadRoleAsync(DefaultIdentity identity, RoleRequest record);
        Task<RoleResponse> AddRoleAsync(DefaultIdentity identity, RoleRequest request);
        Task<RoleResponse> EditRoleAsync(DefaultIdentity identity, RoleRequest record);
        Task<RoleResponse> DeleteRoleAsync(DefaultIdentity identity, RoleRequest record);
	}
}
