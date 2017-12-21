using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using CheckListManager.AccessControl.Logic.Messages.Users;
using CheckListManager.AccessControl.Logic.Services;
using CheckListManager.Common.Data;
using CheckListManager.Common.Data.Exceptions;
using CheckListManager.Common.Logic.Api;
using CheckListManager.Common.Logic.Api.Messages;

namespace CheckListManager.AccessControl.Api.Controllers
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
        [Route("GetData")]
        [Authorize(Roles = "Users#View")]
        public async Task<IHttpActionResult> GetUsers(PagingRequest<UsersRequest> request)
        {
            return Ok(await _userService.GetUsersAsync(Identity.CompanyId, request));
        }

        [HttpPost]
        [Route("Load")]
        [Authorize(Roles = "Users#View")]
        public async Task<IHttpActionResult> Get(Message<UserRequest> request)
        {
            return Ok(await _userService.LoadUserAsync(Identity, request.Data));
        }

        [HttpPost]
        [Route("Add")]
        [Authorize(Roles = "Users#Edit")]
        public async Task<IHttpActionResult> AddUser(Message<UserRequest> request)
        {
            try
            {
                return Ok(await _userService.AddUserAsync(Identity, request.Data));
            }
            catch (DatabaseAggregateException exception)
            {
                return Error(new ResponseMessage(exception.InnerExceptions.Select(e => e.Message).ToArray()));
            }
            catch (DatabaseException exception)
            {
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (exception.Error)
                {
                    case DatabaseError.PrimaryKeyViolation:
                        return Error(new ResponseMessage("An user with the same code already exists."));
                    default:
                        return Error(new ResponseMessage(exception.Message));
                }
            }
        }

        [HttpPost]
        [Route("Edit")]
        [Authorize(Roles = "Users#Edit")]
        public async Task<IHttpActionResult> Edit(Message<UserRequest> request)
        {
            return Ok(await _userService.EditUserAsync(Identity, request.Data));
        }

        [HttpPost]
        [Route("Delete")]
        [Authorize(Roles = "Users#Edit")]
        public async Task<IHttpActionResult> Delete(Message<UserRequest> request)
        {
            return Ok(await _userService.DeleteUserAsync(Identity, request.Data.Code));
        }
    }
}
