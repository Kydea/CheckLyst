using System.Net;
using System;
using System.Threading.Tasks;
using System.Web.Http;
using CheckListManager.Common.Logic.Api;
using CheckListManager.Common.Logic.Api.Formatters;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Instances.Data.Models;
using CheckListManager.Instances.Logic.List;
using CheckListManager.Common.Logic.Cache;
using CheckListManager.Common.Data;
using CheckListManager.Common.Core.Helpers;
using System.Data.Entity.Core.Metadata.Edm;

namespace CheckListManager.Instances.Api.Controllers
{
    [RoutePrefix("List")]
    public class ListController : BaseApiController
    {
        private readonly IListService _listService;
        private readonly IRedisCache _redisCache;
        private readonly ICommonRepository _commonRepository;

        public ListController(IListService listService, IRedisCache redisCache, ICommonRepository commonRepository)
        {
            _listService = listService;
            _redisCache = redisCache;
            _commonRepository = commonRepository;
        }

        [HttpPost]
        [Route("Bind")]
        [Authorize(Roles = "Instances#View, Prototypes#Preview")]
        public async Task<IHttpActionResult> Bind(Message<InstanceFilters> request)
        {
            return Ok(await _listService.BindAsync(Identity, request.Data));
        }

        [HttpPost]
        [Route("GetData")]
        [Authorize(Roles = "Instances#View, Prototypes#Preview")]
        public async Task<IHttpActionResult> GetData(PagingRequest<InstanceFilters> request)
        {
            return Ok(await _listService.GetDataAsync(Identity, request));
        }

        [HttpPost]
        [Route("Overview")]
        [Authorize(Roles = "Instances#View")]
        public async Task<IHttpActionResult> Overview(Message<InstanceFilters> request)
        {
            return Ok(await _listService.OverviewAsync(Identity, request.Data));
        }

        [HttpPost]
        [Route("LifeCycle")]
        [Authorize(Roles = "Instances#View")]
        public async Task<IHttpActionResult> LifeCycle(Message<InstanceFilters> request)
        {
            return Ok(await _listService.LifeCycleAsync(Identity, request.Data));
        }

        [HttpPost]
        [Route("Export")]
        [Authorize(Roles = "Instances#View")]
        public async Task<IHttpActionResult> Export(Message<InstanceFilters> request)
        {
            var Token = Identity.Code + ":" + Guid.NewGuid();
            request.Data.CompanyId = Identity.CompanyId;
            request.Data.User = Identity.Code;
            await _redisCache.SetAsync($"Token:{Token}", request.Data, "Download", new TimeSpan(0, 1, 0));
            return Ok(new { Token });
        }

        [HttpGet]
        [Route("Export")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> Export([FromUri] string Token)
        {
            var data = await _redisCache.GetAsync<InstanceFilters>($"Token:{Token}", "Download");
            var company = await _commonRepository.GetCompanyAsync(data.CompanyId.Value);
            await _redisCache.RemoveAsync($"Token:{Token}", "Download");
            var Header = new string[] {
                "ID",
                "Code",
                "Year",
                "Number",
                "State",
                "Department",
                "DepartmentName",
                "PrototypeType",
                "PrototypeTypeName",
                "Prototype",
                "PrototypeName",
                "PrototypeVersion",
                "LifeDays",
                "PrototypeLifeCycle",
                "LifeCycle",
                "CreatedOn",
                "CreatedBy",
                "ModifiedOn",
                "ModifiedBy"
            };
            var Rows = await _listService.ExportAsync(data);
            return Content(HttpStatusCode.OK, Rows, new CSVFormatter<InstanceRecord>(Header, "Instances", i => new string[] {
                i.Id.ToString(),
                i.Code,
                i.Year,
                i.Number.ToString(), 
                i.State,
                i.Department.ToString(),
                i.DepartmentName,
                i.PrototypeType.ToString(),
                i.PrototypeTypeName,
                i.Prototype.ToString(),
                i.PrototypeName,
                i.Version.ToString(),
                i.LifeDays.ToString(),
                i.PrototypeLifeCycle.ToString(),
                i.LifeCycle,
                i.CreatedOn.Format(company.TimeZone),
                i.CreatedBy,
                i.ModifiedOn?.Format(company.TimeZone),
                i.ModifiedBy
            }));
        }
    }
}
