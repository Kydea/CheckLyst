using System.Threading.Tasks;
using System.Web.Http;
using CheckListManager.AccessControl.Logic.Messages.Access;
using CheckListManager.AccessControl.Logic.Services;
using CheckListManager.Common.Logic.Api;
using CheckListManager.Common.Logic.Api.Messages;
using System;
using CheckListManager.Common.Logic.Authentication;

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
			await _userService.LogoutAsync(Identity, request?.Data?.RefreshToken);
		    return Ok();
	    }

        [HttpPost]
        public async Task<IHttpActionResult> SetPIN(ChangePINRequest request)
        {
            await _userService.SetPINAsync(Identity.CompanyId, Identity.Code, request.Password, request.PIN);
            return Ok();
        }

        [HttpPost]
        public async Task<IHttpActionResult> Authenticate(AuthenticateRequest request)
        {
            await _userService.AuthenticateAsync(Identity.CompanyId, request.Code, request.Password);
            return Ok();
        }

        [HttpPost]
        public async Task<IHttpActionResult> AuthenticateWithPIN(AuthenticateWithPINRequest request)
        {
            await _userService.AuthenticateWithPINAsync(Identity.CompanyId, Identity.Code, request.PIN);
            return Ok();
        }
    }
}
