using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Data;
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
        private readonly ICommonRepository _commonRepository;

        public UsersListController(
            IUsersListService listService,
            IRedisCache redisCache,
            ISettingService settingService,
            ICommonRepository commonRepository
        )
        {
            _listService = listService;
            _redisCache = redisCache;
            _settingService = settingService;
            _commonRepository = commonRepository;
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
            
            var timeZone = (await _settingService.TimeZoneAsync(Data.CompanyId.Value)).MicrosoftCode;
            var rows = await _listService.GetDataAsync(Data.CompanyId.Value, Data);

            var headers = new List<string> {
                "ID",
                "Date",
                "Action",
                "Action By",
                "Affected User",
                "Email",
                "Name And Surname",
                "Enabled",
                "Reason"
            };

            var permissionHeaders = rows
                .SelectMany(r => r.Details.Functions)
                .OrderBy(s => s.Sorting)
                .Select(s => s.FunctionGroup)
                .Distinct()
                .ToList();
            
            headers.AddRange(permissionHeaders);

            var departments = await _commonRepository.GetDepartmentsAsync(Data.CompanyId.Value);
            var prototypeTypes = await _commonRepository.GetPrototypeTypesAsync(Data.CompanyId.Value);
            
            return Content(HttpStatusCode.OK, rows, new CSVFormatter<UsersListResponse>(headers.ToArray(), "AuditTrail",
                tm =>
                {
                    var columns = new List<string>
                    {
                        tm.Id.ToString(),
                        tm.Date.ToLocalTime(timeZone).ToString(),
                        tm.Action,
                        tm.User,
                        tm.AffectedUser,
                        tm.Details.Email,
                        tm.Details.NameAndSurname,
                        !tm.Details.Enabled.HasValue ? "N/A" : tm.Details.Enabled.Value.ToString(),
                        tm.Details.Reason
                    };

                    foreach (var function in tm.Details.Functions)
                        function.SetValues(departments, prototypeTypes);

                    foreach (var permission in permissionHeaders)
                    {
                        var values = tm.Details.Functions.Where(f => f.FunctionGroup == permission);
                        columns.Add(string.Join(" - ", values.Select(v => v.ToString())));
                    }

                    return columns.ToArray();
                }
            ));
        }
    }
}
