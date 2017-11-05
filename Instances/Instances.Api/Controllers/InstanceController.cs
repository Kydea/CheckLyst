using System.Threading.Tasks;
using System.Web.Http;
using CheckListManager.Common.Logic.Api;
using CheckListManager.Instances.Logic.Messages;
using CheckListManager.Instances.Logic.Services;

namespace CheckListManager.Instances.Api.Controllers
{
	[RoutePrefix("Instance")]
	public class InstanceController : BaseApiController
	{
		private readonly IInstanceService _instanceService;

		public InstanceController(IInstanceService instanceService)
		{
			_instanceService = instanceService;
		}

		[HttpGet]
		[Route("Create/{Prototype}/{Version}")]
		public async Task<IHttpActionResult> Create([FromUri]CreateInstanceRequest request)
		{
			return Ok(await _instanceService.Create(request.Prototype, request.Version));
		}
	}
}