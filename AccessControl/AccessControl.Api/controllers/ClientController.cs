using System.Threading.Tasks;
using System.Web.Http;
using CheckListManager.AccessControl.Logic.Messages.Client;
using CheckListManager.AccessControl.Logic.Services;
using CheckListManager.Common.Logic.Api;
using CheckListManager.Common.Logic.Api.Messages;

namespace CheckListManager.AccessControl.Api.Controllers
{
	[RoutePrefix("Client")]
	[Route("{Action}")]
    public class ClientController : BaseApiController
    {
	    private readonly IClientService _clientService;

	    public ClientController(IClientService clientService)
	    {
		    _clientService = clientService;
	    }

		[HttpPost]
		[Route("Create")]
	    public async Task<IHttpActionResult> Create(Message<CreateRequest> request)
		{
			var response = new CreateResponse
			{
				ClientId = await _clientService.CreateAsync(Identity.CompanyId, Identity.Code, request.Data.Token)
			};
			return Ok(Message.Create(response));
		}
    }
}
