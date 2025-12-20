using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Logic.Api;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Reports.Logic.Detail;
using CheckListManager.Reports.Logic.Run;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace CheckListManager.Reports.Api.Controllers
{
    [Authorize(Roles = "FunctionGroup#Reports")]
    [RoutePrefix("Run")]
    public class RunController : BaseApiController
	{
		private readonly IRunService _runService;

		public RunController(IRunService runService)
		{
			_runService = runService;
		}

        [HttpPost]
        [Route("Bind")]
        public async Task<IHttpActionResult> Bind(Message<ListRequest> request)
        {
            return Ok(await _runService.BindAsync(Identity, request.Data));
        }

		[HttpPost]
		[Route("GetData")]
		public async Task<IHttpActionResult> GetData(PagingRequest<ListRequest> request)
		{
            return Ok(await _runService.GetDataAsync(Identity, request));
		}

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(RunRequest request)
        {
            await _runService.DeleteAsync(Identity, request);
            return Ok();
        }
    }
}
