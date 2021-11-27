using CheckListManager.Common.Logic.Api;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.MigrationTools.Api;
using CheckListManager.Prototypes.Logic.Archiving;
using CheckListManager.Prototypes.Logic.List;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace CheckListManager.Prototypes.Api.Controllers
{
    [AllowAnonymous]
    [RoutePrefix("Archiving")]
    public class ArchivingController : BaseApiController
    {
        private static Dictionary<long, Monitor> _monitor = new Dictionary<long, Monitor>();
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
            if (!_monitor.Any())
                _monitor = result.ToDictionary(k => k.Key, v => new Monitor(v.Key, v.Value));
            return Ok(result);
        }

        [HttpPost]
        [Route("Export")]
        public async Task<IHttpActionResult> Export(ArchivingRequest request)
        {
            await _archivingService.ExportAsync(Identity, _monitor[request.Company], request);
            return Json(true);
        }

        [HttpPost]
        [Route("Monitor/{Company}")]
        public IHttpActionResult Monitor([FromUri] long company)
        {
            return Json(_monitor[company].Dump());
        }

        [HttpGet]
        [Route("Abort/{Company}")]
        public IHttpActionResult Abort(long company)
        {
            _monitor[company].Abort();
            return Ok();
        }
    }
}
