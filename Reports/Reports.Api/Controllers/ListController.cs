using CheckListManager.Common.Logic.Api;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Reports.Logic.Detail;
using CheckListManager.Reports.Logic.List;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace CheckListManager.Reports.Api.Controllers
{
    [Authorize(Roles = "FunctionGroup#Reports")]
    [RoutePrefix("List")]
	public class ListController : BaseApiController
	{
		private readonly IListService _listService;
		private readonly IDetailService _detailService;

		public ListController(IListService listService, IDetailService detailService)
		{
			_listService = listService;
			_detailService = detailService;
		}

        [Authorize(Roles = "Reports#View")]
		[HttpPost]
		[Route("GetData")]
		public async Task<IHttpActionResult> GetData(PagingRequest<ListRequest> request)
		{
            return Ok(await _listService.GetDataAsync(Identity.CompanyId, request));
		}

        [Authorize(Roles = "Reports#View")]
        [HttpPost]
        [Route("Load")]
        public async Task<IHttpActionResult> Load(Message<DetailRequest> request)
        {
            return Ok(await _detailService.LoadReportAsync(Identity.CompanyId, request.Data));
        }

        [Authorize(Roles = "Reports#Edit")]
		[HttpPost]
		[Route("Save")]
		public async Task<IHttpActionResult> Save(Message<DetailRequest> request)
		{
			return Ok(await _detailService.SaveReportAsync(Identity, request.Data));
		}

        [Authorize(Roles = "Reports#Edit")]
		[HttpPost]
		[Route("Delete")]
		public async Task<IHttpActionResult> Delete(Message<DetailRequest> request)
		{
			return Ok(await _detailService.DeleteReportAsync(Identity, request.Data));
		}

        [Authorize(Roles = "Reports#Edit")]
        [HttpPost]
        [Route("GetReportSources")]
        public async Task<IHttpActionResult> GetReportSources()
        {
            return Ok(await _detailService.GetReportSourcesAsync());
        }

        [Authorize(Roles = "Reports#Edit")]
        [HttpPost]
        [Route("GetColumns")]
        public async Task<IHttpActionResult> GetColumns(DetailRequest request)
        {
            try
            {
                return Ok(await _detailService.GetColumnsAsync(Identity, request.Query));
            }
            catch (Exception Exception)
            {
                var Response = new ResponseMessage<Exception> { Data = Exception };
                Response.Messages.AddRange(BuildErrorMessage(Exception));
                return new ErrorResult(Request, Response);
            }
        }
    }
}
