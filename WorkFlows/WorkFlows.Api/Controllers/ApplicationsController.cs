using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Logic.Api;
using CheckListManager.Common.Logic.WorkFlows;
using CheckListManager.WorkFlows.Logic.Applications;
using System;
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
        [Authorize(Roles = "Prototypes#Edit, Prototypes#View")]
        public async Task<IHttpActionResult> GetSteps(GetStepsRequest request)
        {
            return Ok(await _applicationService.GetStepsAsync(Identity, request));
        }

        [HttpPost]
        [Route("Create")]
        [Authorize(Roles = "Prototypes#Edit, Instances#Create")]
        public async Task<IHttpActionResult> Create(CreateRequest request)
        {
            try
            {
                return Ok(await _applicationService.CreateAsync(Identity, request));
            }
            catch (Exception Exception)
            {
                return Error(Exception);
            }
        }

        [HttpPost]
        [Route("Sign")]
        [Authorize(Roles = "Prototypes#Approve")]
        public async Task<IHttpActionResult> Sign(SignRequest request)
        {
            try
            {
                return Ok(await _applicationService.SignAsync(Identity, request));
            }
            catch(Exception Exception)
            {
                return Error(Exception);
            }
        }
    }
}
