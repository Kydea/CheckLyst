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
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

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
            request.Data.TimeZone = Request.Headers.GetValues("CompanyTimeZone").Single();
            await _redisCache.SetAsync($"Token:{Token}", request.Data, "Download", new TimeSpan(0, 1, 0));
            return Ok(new { Token = Token });
        }

        private string PrettifyValue(object value)
        {
            return
                value == null ? "(null)" :
                value is string stringValue ? stringValue == string.Empty ? "(empty)" : stringValue :
                value is DateTime dateTimeValue ? dateTimeValue.ToString("yyyy-MM-dd HH:mm") :
                value is DateTimeOffset dateTimeOffsetValue ? dateTimeOffsetValue.ToString("yyyy-MM-dd HH:mm UTC zzz") :
                value.ToString();
        }

        private void PrettifyObject(StringBuilder stringBuilder, JToken token, int indentation)
        {
            try
            {
                if (token is JValue jValue)
                    stringBuilder.Append(PrettifyValue(jValue?.Value));
                else if (token is JArray jArray)
                    for (var index = 0; index < jArray.Count; ++index)
                    {
                        stringBuilder.Append($"\n{new string(' ', 4 * indentation)}{index + 1}: ");
                        PrettifyObject(stringBuilder, jArray[index], indentation + 1);
                    }
                else if (token is JObject jObject)
                    foreach (var child in jObject)
                    {
                        stringBuilder.Append($"\n{new string(' ', 4 * indentation)}{child.Key}: ");
                        PrettifyObject(stringBuilder, child.Value, indentation + 1);
                    }
                else
                    throw new Exception("Unexpected token type: " + token.GetType().Name);
            }
            catch (Exception exception)
            {
                stringBuilder.Append($"\n###Error: {exception.Message}");
            }
        }

        private string PrettifyObject(string value, int indentation)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;

            try
            {
                var node = JsonConvert.DeserializeObject(value);

                if (node is JObject jObject)
                {
                    var stringBuilder = new StringBuilder();
                    PrettifyObject(stringBuilder, jObject, indentation);
                    return stringBuilder.Length == 0 ? null : stringBuilder.ToString(1, stringBuilder.Length - 1); //discards the first character (line break)
                }
                else
                    return PrettifyValue(node);
            }
            catch
            {
                return value;
            }
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
                "Logged user code",
                "Action",
                "CheckList code",
                "Detail"
            };

            var Rows = await _listService.GetDataAsync(Data.CompanyId.Value, Data);
            return Content(HttpStatusCode.OK, Rows, new CSVFormatter<TrailMessage>(Header, "AuditTrail", tm =>
                new string[] {
                    tm.Id.ToString(),
                    tm.Date.ToLocalTime(Data.TimeZone).ToString(),
                    tm.User,
                    tm.Action,
                    tm.CheckList,
                    tm.Information == null ? null : PrettifyObject(tm.Information, 0)
            }));
        }
    }
}
