using CheckListManager.Common.Logic.Api;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.MigrationTools.Api;
using CheckListManager.Prototypes.Logic.Archiving;
using CheckListManager.Prototypes.Logic.List;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace CheckListManager.Prototypes.Api.Controllers
{
    [AllowAnonymous]
    [RoutePrefix("Archiving")]
    public class ArchivingController : BaseApiController
    {
        private readonly IArchivingService _archivingService;

        public ArchivingController(IArchivingService analysisService)
        {
            _archivingService = analysisService;
        }

        [HttpGet]
        [Route("Companies")]
        public async Task<IHttpActionResult> Companies()
        {
            var result = await _archivingService.GetCompaniesAsync();
            return Ok(result);
        }

        [HttpGet]
        [Route("Export")]
        public async Task<IHttpActionResult> Export()
        {
            await _archivingService.InstancesAsync(Identity, null);
            return Ok();
        }

        [HttpGet]
        [Route("Monitor")]
        public IHttpActionResult Monitor()
        {
            return Json(MigrationTools.Api.Monitor.Dump());
        }

        [HttpGet]
        [Route("Abort")]
        public IHttpActionResult Abort()
        {
            MigrationTools.Api.Monitor.Abort();
            return Ok();
        }
    }
}
