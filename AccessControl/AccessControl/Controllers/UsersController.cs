using System.Threading.Tasks;
using System.Web.Http;
using CheckListManager.AccessControl.Logic.Messages.Users;
using CheckListManager.AccessControl.Logic.Services;
using CheckListManager.Common.Logic.Messages;

namespace CheckListManager.AccessControl.Controllers
{
	[RoutePrefix("Users")]
	[Route("{Action}")]
    public class UsersController : ApiController
    {
	    private readonly IUserService _userService;

	    public UsersController(IUserService userService)
	    {
		    _userService = userService;
	    }

		[Route]
		[HttpPost]
	    public async Task<IHttpActionResult> GetUsers(PagingRequest<UsersRequest> request)
		{
			return Ok(await _userService.GetUsersAsync(request));
		}
    }
}
