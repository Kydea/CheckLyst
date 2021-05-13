using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CheckListManager.AccessControl.Logic.Authentication;
using CheckListManager.AccessControl.Logic.Messages.Groups;
using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.AccessControl.Data.Models;

namespace CheckListManager.AccessControl.Logic.Services
{
	public interface IGroupService : IService
	{
		Task<PagingResponse<List<GroupsResponse>>> GetGroupsAsync(long companyId, PagingRequest<GroupsRequest> request);
        Task<List<string>> GetSyntheticDataAsync(long companyId);

        Task < GroupResponse> LoadGroupAsync(DefaultIdentity identity, GroupRequest record);
        Task<GroupResponse> AddGroupAsync(DefaultIdentity identity, GroupRequest request);
        Task<GroupResponse> EditGroupAsync(DefaultIdentity identity, GroupRequest record);
        Task<GroupResponse> DeleteGroupAsync(DefaultIdentity identity, GroupRequest record);
	}
}
