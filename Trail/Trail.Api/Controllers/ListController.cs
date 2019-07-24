using CheckListManager.Common.Logic.Api;
using CheckListManager.Common.Logic.Api.Messages;
using System.Threading.Tasks;
using System.Web.Http;
using CheckListManager.Trail.Logic.Services;
using CheckListManager.Common.Logic.Api.Formatters;
using System.Net;
using System.Linq;
using CheckListManager.Common.Logic.Cache;
using System;
using CheckListManager.Common.Logic.Trail;
using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Logic.Services;

namespace CheckListManager.Trail.Api.Controllers
{
    [RoutePrefix("List")]
    public class ListController : BaseApiController
    {
        private readonly IListService _listService;
        private readonly IRedisCache _redisCache;
        private readonly ISettingService _settingService;

        public ListController(IListService listService, IRedisCache redisCache, ISettingService settingService)
        {
            _listService = listService;
            _redisCache = redisCache;
            _settingService = settingService;
        }

        [HttpPost]
        [Route("GetData")]
        [Authorize(Roles = "Trail#View")]
        public async Task<IHttpActionResult> GetData(PagingRequest<ListRequest> request)
        {
            return Ok(await _listService.GetDataAsync(Identity.CompanyId, request));
        }

        [HttpPost]
        [Route("Export")]
        [Authorize(Roles = "Trail#View")]
        public async Task<IHttpActionResult> Export(Message<ListRequest> request)
        {
            var Token = Identity.Code + ":" + Guid.NewGuid();
            request.Data.CompanyId = Identity.CompanyId;
            await _redisCache.SetAsync($"Token:{Token}", request.Data, "Download", new TimeSpan(0, 1, 0));
            return Ok(new { Token = Token });
        }

        [HttpGet]
        [Route("Export")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> Export([FromUri]string Token)
        {
            var Data = await _redisCache.GetAsync<ListRequest>($"Token:{Token}", "Download");
            await _redisCache.RemoveAsync($"Token:{Token}", "Download");
            var Header = new string[] {
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
