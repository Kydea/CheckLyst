using CheckListManager.Common.Logic.Api;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Reports.Logic.Viewer;
using System.Threading.Tasks;
using System.Web.Http;
using CheckListManager.Common.Logic.DataEngine;
using System;
using CheckListManager.Common.Logic.Cache;
using Newtonsoft.Json;
using System.Collections.Generic;
using CheckListManager.Reports.Logic.Detail;
using System.Threading;
using System.Web;

namespace CheckListManager.Reports.Api.Controllers
{
    [Authorize(Roles = "Reports#View")]
    [RoutePrefix("Viewer")]
    public class ViewerController : BaseApiController
    {
        private readonly IViewerEditorService _reportViewerService;

        public ViewerController(IViewerEditorService reportViewerService)
        {
            _reportViewerService = reportViewerService;
        }

        [HttpPost]
        [Route("LoadParameters")]
        public async Task<IHttpActionResult> LoadParameters(DetailRequest request)
        {
            return Ok(await _reportViewerService.LoadParameters(Identity, request.ID.Value));
        }

        [HttpPost]
        [Route("GetReportData")]
        public async Task<IHttpActionResult> GetReportData(ReportRequest request)
        {
            try
            {
                var cancellationToken = HttpContext.Current.GetOwinContext().Request.CallCancelled;
                return Ok(await _reportViewerService.GetReportData(Identity, request, cancellationToken));
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
