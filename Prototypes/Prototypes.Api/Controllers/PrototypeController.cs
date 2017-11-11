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

		[HttpGet]
		[Route("Get/{Id}/{Version}")]
		public async Task<IHttpActionResult> Get([FromUri]GetPrototypeRequest request)
		{
			return Ok(Message.Create(await _prototypeService.Get(request)));
		}
	}
}