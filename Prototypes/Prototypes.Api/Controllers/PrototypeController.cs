using System.Threading.Tasks;
using System.Web.Http;
using CheckListManager.Common.Logic.Api;
using CheckListManager.Prototypes.Logic.Messages;
using CheckListManager.Prototypes.Logic.Services;
using System.Linq;

namespace CheckListManager.Prototypes.Api.Controllers
{
	[RoutePrefix("Prototype")]
	public class PrototypeController : BaseApiController
	{
		private readonly IPrototypeService _prototypeService;

		public PrototypeController(IPrototypeService prototypeService)
		{
			_prototypeService = prototypeService;
		}

        [HttpPost]
		[Route("Open")]
		public async Task<IHttpActionResult> Open(PrototypeRequest request)
		{
			return Ok(await _prototypeService.OpenAsync(Identity.CompanyId, request));
		}

        [HttpPost]
        [Route("Clone")]
        public async Task<IHttpActionResult> Clone(PrototypeRequest request)
        {
            return Ok(await _prototypeService.CloneAsync(Identity.CompanyId, request));
        }

        [HttpPost]
        [Route("Version")]
        public async Task<IHttpActionResult> Version(PrototypeRequest request)
        {
            return Ok(await _prototypeService.VersionAsync(Identity.CompanyId, request));
        }

        [HttpPost]
        [Route("Create")]
        public async Task<IHttpActionResult> Create(PrototypeRequest request)
        {
            return Ok(await _prototypeService.CreateAsync(Identity.CompanyId, request));
        }

        [HttpPost]
        [Route("VerifyExpressions")]
        public IHttpActionResult VerifyExpressions(PrototypeRequest request)
        {
            return Ok(_prototypeService.VerifyExpressions(request));
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(PrototypeRequest request)
        {
            return Ok(await _prototypeService.SaveAsync(Identity, request));
        }

    }
}