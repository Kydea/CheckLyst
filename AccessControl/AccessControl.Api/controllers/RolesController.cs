using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using CheckListManager.AccessControl.Data.Models;
using CheckListManager.AccessControl.Logic.Messages.Roles;
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
    [RoutePrefix("Roles")]
    [Route("{Action}")]
    public class RolesController : BaseApiController
    {
        private readonly IRoleService _roleService;
        private readonly IRedisCache _redisCache;
        private readonly ICommonRepository _commonRepository;

        public RolesController(IRoleService roleService, IRedisCache redisCache, ICommonRepository commonRepository)
        {
            _roleService = roleService;
            _redisCache = redisCache;
            _commonRepository = commonRepository;
        }

        [HttpPost]
        [Route("GetData")]
        [Authorize(Roles = "Users#View")]
        public async Task<IHttpActionResult> GetData(PagingRequest<RolesRequest> request)
        {
            return Ok(await _roleService.GetRolesAsync(Identity.CompanyId, request));
        }

        [HttpPost]
        [Route("Load")]
        [Authorize(Roles = "Users#View")]
        public async Task<IHttpActionResult> Load(Message<RoleRequest> request)
        {
            return Ok(await _roleService.LoadRoleAsync(Identity, request.Data));
        }

        [HttpPost]
        [Route("Add")]
        [Authorize(Roles = "Users#Edit")]
        public async Task<IHttpActionResult> AddRole(Message<RoleRequest> request)
        {
            return Ok(await _roleService.AddRoleAsync(Identity, request.Data));
        }

        [HttpPost]
        [Route("Edit")]
        [Authorize(Roles = "Users#Edit")]
        public async Task<IHttpActionResult> Edit(Message<RoleRequest> request)
        {
            return Ok(await _roleService.EditRoleAsync(Identity, request.Data));
        }

        [HttpPost]
        [Route("Delete")]
        [Authorize(Roles = "Users#Edit")]
        public async Task<IHttpActionResult> Delete(Message<RoleRequest> request)
        {
            return Ok(await _roleService.DeleteRoleAsync(Identity, request.Data));
        }

        [HttpPost]
        [Route("GetSyntheticData")]
        [Authorize(Roles = "Users#View")]
        public async Task<IHttpActionResult> GetSyntheticData()
        {
            return Ok(await _roleService.GetSyntheticDataAsync(Identity.CompanyId));
        }

        [HttpPost]
        [Route("Export")]
        [Authorize(Roles = "Users#View")]
        public async Task<IHttpActionResult> Export(Message<RolesRequest> request)
        {
            var Token = Identity.Code + ":" + Guid.NewGuid();
            request.Data.CompanyId = Identity.CompanyId;
            await _redisCache.SetAsync($"Token:{Token}", request.Data, "Download", new TimeSpan(0, 1, 0));
            return Ok(new { Token });
        }

        [HttpGet]
        [Route("Export")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> Export([FromUri]string Token)
        {
            var Data = await _redisCache.GetAsync<RolesRequest>($"Token:{Token}", "Download");
            var Company = await _commonRepository.GetCompanyAsync(Data.CompanyId.Value);
            await _redisCache.RemoveAsync($"Token:{Token}", "Download");
            var Header = new string[] {
                "Code",
                "Function group",
                "Function"
            };
            var Rows = await _roleService.GetFunctionsAsync(Data.CompanyId.Value, Data);
            return Content(HttpStatusCode.OK, Rows, new CSVFormatter<ExportRow>(Header, "Roles", u => new string[] {
                u.Code,
                u.FunctionGroup,
                u.Function
            }));
        }

    }
}
