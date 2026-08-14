using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Data;
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
        private readonly ICommonRepository _commonRepository;

        public ApplicationsController(IApplicationService applicationService, ICommonRepository commonRepository)
        {
            _applicationService = applicationService;
            _commonRepository = commonRepository;
        }

        [HttpPost]
        [Route("GetSteps")]
        [Authorize(Roles = "Prototypes#View, Instances#View, Reports#Edit")]
        public async Task<IHttpActionResult> GetSteps(GetStepsRequest request)
        {
            return Ok(await _applicationService.GetStepsAsync(Identity, request));
        }

        [HttpPost]
        [Route("Create")]
        [Authorize(Roles = "Prototypes#Preview, Instances#Edit, Reports#Edit")]
        public async Task<IHttpActionResult> Create(CreateRequest request)
        {
            return Ok(await _applicationService.CreateAsync(Identity, request));
        }

        [HttpPost]
        [Route("Sign")]
        [Authorize(Roles = "Prototypes#Approve, Instances#Confirm, Reports#Edit")]
        public async Task<IHttpActionResult> Sign(SignRequest request)
        {
            var Company = await _commonRepository.GetCompanyAsync(Identity.CompanyId);
            return Ok(await _applicationService.SignAsync(Identity, request, Company.TimeZone));
        }
    }
}
