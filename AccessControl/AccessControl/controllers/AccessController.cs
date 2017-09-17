using System.Threading.Tasks;
using System.Web.Http;
using CheckListManager.AccessControl.Logic.Messages.Logout;
using CheckListManager.AccessControl.Logic.Services;
using CheckListManager.Common.Logic.Messages;

namespace CheckListManager.AccessControl.Controllers
{
	[RoutePrefix("Access")]
	[Route("{Action}")]
    public class AccessController : ApiController
    {
	    private readonly IUserService _userService;

	    public AccessController(IUserService userService)
	    {
		    _userService = userService;
	    }

		[HttpPost]
	    public async Task<IHttpActionResult> Logout([FromBody]Message<LogoutRequest> request)
		{
			await _userService.LogoutAsync(request?.Data?.RefreshToken);
		    return Ok();
	    }
    }
}
