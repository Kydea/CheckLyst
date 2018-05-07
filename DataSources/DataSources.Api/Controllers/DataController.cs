using CheckListManager.Common.Logic.Api;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.DataSources.Logic.Data;
using System.Threading.Tasks;
using System.Web.Http;
using CheckListManager.Common.Logic.DataEngine;
using System;

namespace CheckListManager.DataSources.Api.Controllers
{
	[RoutePrefix("Data")]
	public class DataController : BaseApiController
	{
		private readonly IDataService _dataService;

		public DataController(IDataService dataService)
		{
            _dataService = dataService;
		}

        [Authorize(Roles = "Instances#Edit, Prototypes#View")]
        [HttpPost]
        [Route("GetDataSource")]
        public async Task<IHttpActionResult> GetDataSource(DataRequest request)
        {
            return Ok(await _dataService.GetDataSourceAsync(Identity.CompanyId, request.Code)); 
        }

        [Authorize(Roles = "Prototypes#View")]
        [HttpPost]
        [Route("GetDataSources")]
        public async Task<IHttpActionResult> GetDataSources(PagingRequest<DataRequest> request)
        {
            return Ok(await _dataService.GetDataSourcesAsync(Identity.CompanyId, request));
        }

        [Authorize(Roles = "DataSources#View")]
        [HttpPost]
        [Route("Test")]
        public async Task<IHttpActionResult> Test(DataRequest request)
        {
            try
            {
                await _dataService.TestAsync(Identity.CompanyId, request.Code);
                return Ok();
            }
            catch(Exception Exception)
            {
                return Error(Exception);
            }
        }

        [Authorize(Roles = "Instances#Edit")]
        [HttpPost]
        [Route("GetDataSourceRows")]
        public async Task<IHttpActionResult> GetDataSourceRows(PagingRequest<DataRequest> request)
        {
            return Ok(await _dataService.GetDataSourceRowsAsync(Identity.CompanyId, request));
        }
    }
}
