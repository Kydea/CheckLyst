using CheckListManager.Common.Logic.Api;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.DataSources.Logic.Detail;
using CheckListManager.DataSources.Logic.List;
using System.Threading.Tasks;
using System.Web.Http;

namespace CheckListManager.DataSources.Api.Controllers
{
	[Authorize(Roles = "FunctionGroup#DataSources")]
	[RoutePrefix("List")]
	public class DataSourcesController : BaseApiController
	{
		private readonly IListService _listService;
		private readonly IDetailService _detailService;

		public DataSourcesController(IListService listService, IDetailService detailService)
		{
			_listService = listService;
			_detailService = detailService;
		}

		[Authorize(Roles = "DataSources#View")]
		[HttpPost]
		[Route("GetData")]
		public async Task<IHttpActionResult> GetData(PagingRequest<ListRequest> request)
		{
            return Ok(await _listService.GetDataAsync(Identity.CompanyId, request));
		}

		[Authorize(Roles = "DataSources#Edit")]
		[HttpPost]
		[Route("Add")]
		public async Task<IHttpActionResult> Add(Message<DetailRequest> request)
		{
			return Ok(await _detailService.AddAsync(Identity, request.Data));
		}

		[Authorize(Roles = "DataSources#Edit")]
		[HttpPost]
		[Route("Edit")]
		public async Task<IHttpActionResult> Edit(Message<DetailRequest> request)
		{
			return Ok(await _detailService.EditAsync(Identity, request.Data));
		}

		[Authorize(Roles = "DataSources#Edit")]
		[HttpPost]
		[Route("Delete")]
		public async Task<IHttpActionResult> Delete(Message<DetailRequest> request)
		{
			return Ok(await _detailService.DeleteAsync(Identity, request.Data.Code));
		}
	}
}
