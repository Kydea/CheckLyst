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
        [Route("Continue")]
        public async Task<IHttpActionResult> Continue()
        {
            await _instancesAnalysisService.ContinueAsync();
            return Ok();
        }

        [HttpGet]
        [Route("Restart")]
        public async Task<IHttpActionResult> Restart()
        {
            await _instancesAnalysisService.RestartAsync();
            return Ok();
        }

        [HttpGet]
        [Route("Stop")]
        public async Task<IHttpActionResult> Stop()
        {
            await _instancesAnalysisService.StopAsync();
            return Ok();
        }

        [HttpGet]
        [Route("Update")]
        public async Task<IHttpActionResult> Update()
        {
            var state = await _instancesAnalysisService.UpdateAsync();
            return Json(state);
        }

    }
}
