using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Logic.Api;
using CheckListManager.Common.Logic.Api.Formatters;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.Cache;
using CheckListManager.Common.Logic.Services;
using CheckListManager.Common.Logic.Trail;
using CheckListManager.Trail.Logic.Services;

namespace CheckListManager.Trail.Api.Controllers
{
    [RoutePrefix("UsersList")]
    public class UsersListController : BaseApiController
    {
        private readonly IUsersListService _listService;
        private readonly IRedisCache _redisCache;
        private readonly ISettingService _settingService;

        public UsersListController(IUsersListService listService, IRedisCache redisCache, ISettingService settingService)
        {
            _listService = listService;
            _redisCache = redisCache;
            _settingService = settingService;
        }

        [HttpPost]
        [Route("GetData")]
        [Authorize(Roles = "Trail#View")]
        public async Task<IHttpActionResult> GetData(PagingRequest<UsersListRequest> request)
        {
            return Ok(await _listService.GetDataAsync(Identity.CompanyId, request));
        }

        [HttpPost]
        [Route("Export")]
        [Authorize(Roles = "Trail#View")]
        public async Task<IHttpActionResult> Export(Message<UsersListRequest> request)
        {
            var Token = Identity.Code + ":" + Guid.NewGuid();
            request.Data.CompanyId = Identity.CompanyId;
            await _redisCache.SetAsync($"Token:{Token}", request.Data, "Download", new TimeSpan(0, 1, 0));
            return Ok(new { Token = Token });
        }

        [HttpGet]
        [Route("Export")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> Export([FromUri]string token)
        {
            var Data = await _redisCache.GetAsync<UsersListRequest>($"Token:{token}", "Download");
            await _redisCache.RemoveAsync($"Token:{token}", "Download");
            var Header = new [] {
                "ID",
                "Date",
                "User",
                "Action",
                "CheckList code",
                "Detail"
            };
            string timeZone = (await _settingService.TimeZoneAsync(Data.CompanyId.Value)).MicrosoftCode;
                
            var Rows = await _listService.GetDataAsync(Data.CompanyId.Value, Data);
            return Content(HttpStatusCode.OK, Rows, new CSVFormatter<TrailMessage>(Header, "AuditTrail", tm => new string[] {
                tm.Id.ToString(),
                tm.Date.ToLocalTime(timeZone).ToString(),
                tm.User,
                tm.Action,
                tm.CheckList,
                tm.Information
            }));
        }
    }
}
