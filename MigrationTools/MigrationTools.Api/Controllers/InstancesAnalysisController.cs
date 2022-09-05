using CheckListManager.Common.Logic.Api;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.MigrationTools.Api;
using CheckListManager.Instances.Logic.InstancesAnalysis;
using CheckListManager.Prototypes.Logic.List;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace CheckListManager.Instances.Api.Controllers
{
    [AllowAnonymous]
    [RoutePrefix("InstancesAnalysis")]
    public class InstancesAnalysisController : BaseApiController
    {
        private readonly IInstancesAnalysisService _instancesAnalysisService;
        private static readonly Monitor _monitor = new Monitor();

        public InstancesAnalysisController(IInstancesAnalysisService instancesAnalysisService)
        {
            _instancesAnalysisService = instancesAnalysisService;
        }

        [HttpGet]
        [Route("Start")]
        public async Task<IHttpActionResult> Start()
        {
            await _instancesAnalysisService.StartAsync(Identity, _monitor, null);
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
