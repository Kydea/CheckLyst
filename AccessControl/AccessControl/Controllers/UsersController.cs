using System.Threading.Tasks;
using System.Web.Http;
using CheckListManager.AccessControl.Logic.Messages.Users;
using CheckListManager.AccessControl.Logic.Services;
using CheckListManager.Common.Logic;
using CheckListManager.Common.Logic.Messages;

namespace CheckListManager.AccessControl.Controllers
{
	[RoutePrefix("Users")]
	[Route("{Action}")]
    public class UsersController : BaseApiController
    {
	    private readonly IUserService _userService;

	    public UsersController(IUserService userService)
	    {
		    _userService = userService;
	    }

	    [HttpPost]
		[Route]
	    public async Task<IHttpActionResult> GetUsers(PagingRequest<UsersRequest> request)
		{
			return Ok(await _userService.GetUsersAsync(Identity.CompanyId, request));
		}

		[HttpPost]
		[Route("Add")]
	    public async Task<IHttpActionResult> AddUser(Message<AddUserRequest> request)
		{
			var response = await _userService.AddUserAsync(Identity.CompanyId, request.Data);
			return Ok(Message.Create(response));
		}
    }
}
