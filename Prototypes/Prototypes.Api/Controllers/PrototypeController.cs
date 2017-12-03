using System.Threading.Tasks;
using System.Web.Http;
using CheckListManager.Common.Logic.Api;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Prototypes.Logic.Messages;
using CheckListManager.Prototypes.Logic.Services;

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
		[Route("Open/{Id}/{Version}")]
		public async Task<IHttpActionResult> Open(GetPrototypeRequest request)
		{
			return Ok(await _prototypeService.OpenAsync(Identity.CompanyId, request));
		}

        [HttpPost]
        [Route("Clone/{Id}/{Version}")]
        public async Task<IHttpActionResult> Clone(GetPrototypeRequest request)
        {
            return Ok(await _prototypeService.CloneAsync(Identity.CompanyId, request));
        }

        [HttpPost]
        [Route("Version/{Id}/{Version}")]
        public async Task<IHttpActionResult> Version(GetPrototypeRequest request)
        {
            return Ok(await _prototypeService.VersionAsync(Identity.CompanyId, request));
        }

        [HttpPost]
        [Route("Create")]
        public async Task<IHttpActionResult> Create(GetPrototypeRequest request)
        {
            return Ok(await _prototypeService.CreateAsync(Identity.CompanyId, request));
        }
    }
}