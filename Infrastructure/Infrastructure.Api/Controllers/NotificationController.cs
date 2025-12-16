using CheckListManager.Infrastructure.Logic.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Threading;
using System.Web;
using System.Web.Http;
using CheckListManager.Common.Logic.Api;
using CheckListManager.Common.Logic.Api.Filters;

namespace CheckListManager.Infrastructure.Api.Controllers
{
    namespace YourApp.Controllers
    {
        [Authorize]
        [RoutePrefix("Notifications")]
        public class NotificationsController : BaseApiController
        {
            private readonly INotificationService _notificationService;

            public NotificationsController(INotificationService notificationService)
            {
                _notificationService = notificationService;
            }

            [HttpGet]
            [NoCache]
            [Route("Polling")]
            public async Task<IHttpActionResult> Polling([FromUri] ulong since)
            {
                var requestToken = HttpContext.Current.GetOwinContext().Request.CallCancelled;
                NotificationSnapshot snap;
                try
                {
                    snap = await _notificationService.PollingAsync(
                        Identity.CompanyId,
                        Identity.Code,
                        since,
                        requestToken
                    ).ConfigureAwait(false);

                    return Ok(snap);
                }
                catch (TaskCanceledException)
                {
                    return StatusCode(System.Net.HttpStatusCode.NoContent);
                }
            }

            [HttpGet]
            [Authorize(Roles = "Companies#Edit")]
            [Route("Diagnostics")]
            public IHttpActionResult Diagnostics()
            {
                return Ok(_notificationService.GetDiagnostics());
            }

            [AllowAnonymous]
            [HttpGet]
            [Route("Test/{Task}/{Report}")]
            public IHttpActionResult Test([FromUri] long Task, [FromUri] long Report)
            {
                var snap = _notificationService.Update(2, "CheckListManager", state =>
                {
                    state.TaskCount += Task;
                    state.ReportCount += Report;
                });

                return Ok(snap);
            }
        }
    }
}