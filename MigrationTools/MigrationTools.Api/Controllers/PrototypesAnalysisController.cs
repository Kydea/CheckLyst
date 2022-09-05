using CheckListManager.Common.Logic.Api;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.MigrationTools.Api;
using CheckListManager.Prototypes.Logic.PrototypesAnalysis;
using CheckListManager.Prototypes.Logic.List;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace CheckListManager.Prototypes.Api.Controllers
{
    [AllowAnonymous]
    [RoutePrefix("PrototypesAnalysis")]
    public class PrototypesAnalysisController : BaseApiController
    {
        private readonly IPrototypesAnalysisService _prototypesAnalysisService;
        private static readonly Monitor _monitor = new Monitor();

        public PrototypesAnalysisController(IPrototypesAnalysisService prototypesAnalysisService)
        {
            _prototypesAnalysisService = prototypesAnalysisService;
        }

        [HttpGet]
        [Route("Start")]
        public async Task<IHttpActionResult> Start()
        {
            await _prototypesAnalysisService.StartAsync(Identity, _monitor, null);
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
