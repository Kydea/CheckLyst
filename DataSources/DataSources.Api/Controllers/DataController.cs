using CheckListManager.Common.Logic.Api;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.DataSources.Logic.Data;
using System.Threading.Tasks;
using System.Web.Http;
using CheckListManager.Common.Logic.DataEngine;
using System;
using CheckListManager.Common.Logic.Cache;
using CheckListManager.Common.Logic.DataSources;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CheckListManager.DataSources.Api.Controllers
{
	[RoutePrefix("Data")]
	public class DataController : BaseApiController
	{
		private readonly IDataService _dataService;
        private readonly IRedisCache _redisCache;

        public DataController(IDataService dataService, IRedisCache redisCache)
		{
            _dataService = dataService;
            _redisCache = redisCache;
        }

        [Authorize(Roles = "Instances#Edit, Prototypes#View")]
        [HttpPost]
        [Route("GetDataSource")]
        public async Task<IHttpActionResult> GetDataSource(Logic.Data.DataRequest request)
        {
            try
            {
                return Ok(await _dataService.GetDataSourceAsync(Identity.CompanyId, request.Code));
            }
            catch (Exception Exception)
            {
                return Error(Exception);
            }
        }

        [Authorize(Roles = "Prototypes#View, DataSources#Edit")]
        [HttpPost]
        [Route("GetDataSources")]
        public async Task<IHttpActionResult> GetDataSources(PagingRequest<Logic.Data.DataRequest> request)
        {
            return Ok(await _dataService.GetDataSourcesAsync(Identity.CompanyId, request));
        }

        [HttpPost]
        [Route("GetDataSourceRows")]
        public async Task<IHttpActionResult> GetDataSourceRows(TokenRequest tokenRequest)
        {
            var Data = await _redisCache.GetAsync<Common.Logic.DataSources.DataRequest>($"Token:{tokenRequest.token}", "S2S");
            await _redisCache.RemoveAsync($"Token:{tokenRequest.token}", "S2S");

            return Ok(await _dataService.GetDataSourceRowsAsync(Identity.CompanyId, Data));
        }

        [Authorize(Roles = "DataSources#Edit")]
        [HttpPost]
        [Route("Test")]
        public async Task<IHttpActionResult> Test(Common.Logic.DataSources.DataRequest request)
        {
            try
            {
                var JsonString = await _dataService.GetDataSourceRowsAsync(Identity.CompanyId, request);
                return Ok(JsonString);
            }
            catch (Exception Exception)
            {
                return Error(Exception);
            }
        }

        [HttpPost]
        [Route("Export")]
        public async Task<IHttpActionResult> Export(TokenRequest tokenRequest)
        {
            var Data = await _redisCache.GetAsync<ExportRequest>($"Token:{tokenRequest.token}", "S2S");
            await _redisCache.RemoveAsync($"Token:{tokenRequest.token}", "S2S");

            await _dataService.ExportAsync(Identity.CompanyId, Data);

            return Ok();
        }
    }
}
