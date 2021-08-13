using CheckListManager.Common.Logic.Api;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Prototypes.Logic.Analysis;
using CheckListManager.Prototypes.Logic.List;
using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace CheckListManager.Prototypes.Api.Controllers
{
    [AllowAnonymous]
    public class AnalysisController : BaseApiController
    {
        private readonly IAnalysisService _analysisService;

        public AnalysisController(IAnalysisService analysisService)
        {
            _analysisService = analysisService;
        }

        [HttpGet]
        [Route("Prototypes")]
        public async Task<IHttpActionResult> Prototypes()
        {
            try
            {
                var cancellationToken = HttpContext.Current.GetOwinContext().Request.CallCancelled;
                var response = await _analysisService.PrototypesAsync(Identity, null, cancellationToken);
                return Json(response);
            }
            catch (Exception e)
            {
                return Ok(e?.Message + Environment.NewLine + e?.StackTrace);
            }
        }
    }
}
