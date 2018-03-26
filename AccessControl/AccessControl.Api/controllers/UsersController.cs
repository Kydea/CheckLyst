using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using CheckListManager.AccessControl.Logic.Messages.Users;
using CheckListManager.AccessControl.Logic.Services;
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

        public UsersController(IUserService userService, IRedisCache redisCache)
        {
            _userService = userService;
            _redisCache = redisCache;
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
            try
            {
                return Ok(await _userService.AddUserAsync(Identity, request.Data));
            }
            catch(Exception Exception)
            {
                return Error(Exception);
            }
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
            return Ok(await _userService.DeleteUserAsync(Identity, request.Data.Code));
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
            await _redisCache.RemoveAsync($"Token:{Token}", "Download");
            var Header = new string[] {
                "Code",
                "Name and surname",
                "Email",
                "Enabled",
                "Last login",
                "Department",
                "Department name",
                "Prototype type",
                "Prototype type name",
                "Function group",
                "Function",
                "Level"
            };
            var Rows = await _userService.GetFunctionsAsync(Data.CompanyId.Value, Data);
            return Content(HttpStatusCode.OK, Rows, new CSVFormatter<UserFunctions>(Header, "Users", u => new string[] {
                u.User,
                u.Users.Description,
                u.Users.Email,
                u.Users.Enabled ? "true" : "false",
                u.Users.LastLoginOn.ToString(),
                u.Department?.ToString(),
                u.Departments?.Name.ToString(),
                u.PrototypeType?.ToString(),
                u.PrototypeTypes?.Name.ToString(),
                u.FunctionGroup,
                u.Function,
                u.Tag
            }));
        }
    }
}
