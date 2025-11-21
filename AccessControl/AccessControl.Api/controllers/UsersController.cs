using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using CheckListManager.AccessControl.Data.Models;
using CheckListManager.AccessControl.Logic.Messages.Users;
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
    [RoutePrefix("Users")]
    [Route("{Action}")]
    public class UsersController : BaseApiController
    {
        private readonly IUserService _userService;
        private readonly IRedisCache _redisCache;
        private readonly ICommonRepository _commonRepository;

        public UsersController(IUserService userService, IRedisCache redisCache, ICommonRepository commonRepository)
        {
            _userService = userService;
            _redisCache = redisCache;
            _commonRepository = commonRepository;
        }

        [HttpPost]
        [Route("Bind")]
        [Authorize(Roles = "Users#View")]
        public async Task<IHttpActionResult> Bind(Message<UsersRequest> request)
        {
            return Ok(await _userService.BindAsync(request.Data));
        }

        [HttpPost]
        [Route("GetData")]
        [Authorize(Roles = "Users#View")]
        public async Task<IHttpActionResult> GetUsers(PagingRequest<UsersRequest> request)
        {
            return Ok(await _userService.GetUsersAsync(Identity.CompanyId, request));
        }

        [HttpPost]
        [Route("Load")]
        [Authorize(Roles = "Users#View")]
        public async Task<IHttpActionResult> Get(Message<UserRequest> request)
        {
            return Ok(await _userService.LoadUserAsync(Identity, request.Data));
        }

        [HttpPost]
        [Route("Add")]
        [Authorize(Roles = "Users#Edit")]
        public async Task<IHttpActionResult> AddUser(Message<UserRequest> request)
        {
            return Ok(await _userService.AddUserAsync(Identity, request.Data));
        }

        [HttpPost]
        [Route("Edit")]
        [Authorize(Roles = "Users#Edit")]
        public async Task<IHttpActionResult> Edit(Message<UserRequest> request)
        {
            return Ok(await _userService.EditUserAsync(Identity, request.Data));
        }

        [HttpPost]
        [Route("Delete")]
        [Authorize(Roles = "Users#Edit")]
        public async Task<IHttpActionResult> Delete(Message<UserRequest> request)
        {
            return Ok(await _userService.DeleteUserAsync(Identity, request.Data));
        }

        [HttpPost]
        [Route("GetConflicts")]
        [Authorize(Roles = "Users#Edit")]
        public async Task<IHttpActionResult> GetConflicts(UserRequest request)
        {
            return Ok(await _userService.GetConflictsAsync(Identity, request.Groups));
        }

        [HttpPost]
        [Route("SetTimeZone")]
        public async Task<IHttpActionResult> SetTimeZone(Message<string> request)
        {
            await _userService.SetTimeZoneAsync(Identity, request.Data);
            return Ok();
        }

        [HttpPost]
        [Route("Export")]
        [Authorize(Roles = "Users#View")]
        public async Task<IHttpActionResult> Export(Message<UsersRequest> request)
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
            var Data = await _redisCache.GetAsync<UsersRequest>($"Token:{Token}", "Download");
            var Company = await _commonRepository.GetCompanyAsync(Data.CompanyId.Value);
            await _redisCache.RemoveAsync($"Token:{Token}", "Download");
            var Header = new string[] {
                "Code",
                "Name and surname",
                "E-mail",
                "Corporate ID",
                "Enabled",
                "Last login",
                "Group",
                "Department ID",
                "Department name",
                "Prototype type ID",
                "Prototype type name",
                "Role",
                "Function group",
                "Function",
                "Signature Level",
                "Prototype Worflow Level",
                "Instance Worflow Level"
            };
            var Rows = await _userService.GetFunctionsAsync(Data.CompanyId.Value, Data);
            return Content(HttpStatusCode.OK, Rows, new CSVFormatter<ExportRow>(Header, "Users", u => new string[] {
                u.Code,
                u.Description,
                u.Email,
                u.CorporateID,
                u.Enabled ? "true" : "false",
                u.LastLoginOn?.Format(Company.TimeZone),
                u.Group,
                u.Department?.ToString(),
                u.DepartmentName,
                u.PrototypeType?.ToString(),
                u.PrototypeTypeName,
                u.Role,
                u.FunctionGroup,
                u.Function,
                u.SignatureLevel,
                u.PrototypeWorkFlowLevel,
                u.InstanceWorkFlowLevel
            }));
        }
    }
}
