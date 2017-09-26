using System.Threading.Tasks;
using System.Web.Http;
using CheckListManager.AccessControl.Logic.Messages.Access;
using CheckListManager.AccessControl.Logic.Services;
using CheckListManager.Common.Logic.Api;
using CheckListManager.Common.Logic.Api.Messages;

namespace CheckListManager.AccessControl.Api.Controllers
{
	[RoutePrefix("Access")]
	[Route("{Action}")]
    public class AccessController : BaseApiController
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
