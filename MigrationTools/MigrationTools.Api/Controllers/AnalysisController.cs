using CheckListManager.Common.Logic.Api;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.MigrationTools.Api;
using CheckListManager.Prototypes.Logic.Analysis;
using CheckListManager.Prototypes.Logic.List;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace CheckListManager.Prototypes.Api.Controllers
{
    [AllowAnonymous]
    [RoutePrefix("Analysis")]
    public class AnalysisController : BaseApiController
    {
        private readonly IAnalysisService _analysisService;
        private static readonly Monitor _monitor = new Monitor();

        public AnalysisController(IAnalysisService analysisService)
        {
            _analysisService = analysisService;
        }

        [HttpGet]
        [Route("Prototypes")]
        public async Task<IHttpActionResult> Prototypes()
        {
            await _analysisService.PrototypesAsync(Identity, _monitor, null);
            return Ok();
        }

        [HttpGet]
        [Route("Monitor")]
        public IHttpActionResult Monitor()
        {
            return Json(_monitor.Dump());
        }

        [HttpGet]
        [Route("Abort")]
        public IHttpActionResult Abort()
        {
            _monitor.Abort();
            return Ok();
        }
    }
}
