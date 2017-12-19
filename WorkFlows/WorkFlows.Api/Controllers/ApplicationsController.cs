using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Logic.Api;
using CheckListManager.WorkFlows.Logic.Applications;
using System.Threading.Tasks;
using System.Web.Http;

namespace CheckListManager.WorkFlows.Api.Controllers
{
    [RoutePrefix("Applications")]
    public class ApplicationsController : BaseApiController
    {
        private readonly IApplicationService _applicationService;

        public ApplicationsController(IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        [HttpPost]
        [Route("GetSteps")]
        public async Task<IHttpActionResult> GetSteps(GetStepsRequest request)
        {
            return Ok(await _applicationService.GetStepsAsync(Identity, request));
        }
    }
}
