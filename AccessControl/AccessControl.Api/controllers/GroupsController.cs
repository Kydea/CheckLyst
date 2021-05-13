using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using CheckListManager.AccessControl.Data.Models;
using CheckListManager.AccessControl.Logic.Messages.Groups;
using CheckListManager.AccessControl.Logic.Services;
using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Data;
using CheckListManager.Common.Data.Exceptions;
using CheckListManager.Common.Logic.Api;
using CheckListManager.Common.Logic.Api.Formatters;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.Cache;

namespace CheckListManager.AccessControl.Api.Controllers
{
    [RoutePrefix("Groups")]
    [Route("{Action}")]
    public class GroupsController : BaseApiController
    {
        private readonly IGroupService _groupService;
        private readonly IRedisCache _redisCache;
        private readonly ICommonRepository _commonRepository;

        public GroupsController(IGroupService groupService, IRedisCache redisCache, ICommonRepository commonRepository)
        {
            _groupService = groupService;
            _redisCache = redisCache;
            _commonRepository = commonRepository;
        }

        [HttpPost]
        [Route("GetData")]
        [Authorize(Roles = "Users#View")]
        public async Task<IHttpActionResult> GetData(PagingRequest<GroupsRequest> request)
        {
            return Ok(await _groupService.GetGroupsAsync(Identity.CompanyId, request));
        }

        [HttpPost]
        [Route("Load")]
        [Authorize(Roles = "Users#View")]
        public async Task<IHttpActionResult> Load(Message<GroupRequest> request)
        {
            return Ok(await _groupService.LoadGroupAsync(Identity, request.Data));
        }

        [HttpPost]
        [Route("Add")]
        [Authorize(Roles = "Users#Edit")]
        public async Task<IHttpActionResult> AddGroup(Message<GroupRequest> request)
        {
            return Ok(await _groupService.AddGroupAsync(Identity, request.Data));
        }

        [HttpPost]
        [Route("Edit")]
        [Authorize(Roles = "Users#Edit")]
        public async Task<IHttpActionResult> Edit(Message<GroupRequest> request)
        {
            return Ok(await _groupService.EditGroupAsync(Identity, request.Data));
        }

        [HttpPost]
        [Route("Delete")]
        [Authorize(Roles = "Users#Edit")]
        public async Task<IHttpActionResult> Delete(Message<GroupRequest> request)
        {
            return Ok(await _groupService.DeleteGroupAsync(Identity, request.Data));
        }

        [HttpPost]
        [Route("GetSyntheticData")]
        [Authorize(Roles = "Users#View")]
        public async Task<IHttpActionResult> GetSyntheticData()
        {
            return Ok(await _groupService.GetSyntheticDataAsync(Identity.CompanyId));
        }
    }
}
