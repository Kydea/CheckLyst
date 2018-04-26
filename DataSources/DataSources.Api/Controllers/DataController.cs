using CheckListManager.Common.Logic.Api;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.DataSources.Logic.Detail;
using CheckListManager.DataSources.Logic.Data;
using System.Threading.Tasks;
using System.Web.Http;

namespace CheckListManager.DataSources.Api.Controllers
{
	[Authorize(Roles = "FunctionGroup#DataSources")]
	[RoutePrefix("Data")]
	public class DataController : BaseApiController
	{
		private readonly IDataService _dataService;

		public DataController(IDataService dataService)
		{
            _dataService = dataService;
		}

		[Authorize(Roles = "Prototypes#Edit")]
		[HttpPost]
		[Route("GetColumns")]
		public async Task<IHttpActionResult> GetColumns(DetailRequest request)
		{
            return Ok(); //await _dataService.GetDataAsync(Identity.CompanyId, request));
		}

        [Authorize(Roles = "Isntances#Edit")]
        [HttpPost]
        [Route("GetRows")]
        public async Task<IHttpActionResult> GetRows(PagingRequest<DetailRequest> request)
        {
            return Ok(); // await _dataService.GetDataAsync(Identity.CompanyId, request));
        }
    }
}
