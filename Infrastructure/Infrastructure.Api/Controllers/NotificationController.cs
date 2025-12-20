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
using CheckListManager.Common.Logic.Models.Notifications;
using CheckListManager.Common.Logic.Models.Expressions.Parsing;
using CheckListManager.Common.Logic.Cache;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Logic.Models;
using CheckListManager.Infrastructure.Data;

namespace CheckListManager.Infrastructure.Api.Controllers
{
    namespace YourApp.Controllers
    {
        [Authorize]
        [RoutePrefix("Notifications")]
        public class NotificationsController : BaseApiController
        {
            private readonly INotificationService _notificationService;
            private readonly IInfrastructureRepository _infrastructureRepository;
            private readonly IRedisCache _redisCache;

            public NotificationsController(
                INotificationService notificationService,
                IInfrastructureRepository infrastructureRepository,
                IRedisCache redisCache
            )
            {
                _notificationService = notificationService;
                _infrastructureRepository = infrastructureRepository;
                _redisCache = redisCache;
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
                        () => _infrastructureRepository.GetNotificationsAsync(Identity.CompanyId, Identity.Code),
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
            [Route("Invalidate")]
            public async Task<IHttpActionResult> Invalidate([FromUri] string token)
            {
                var request = await _redisCache.GetAsync<User>($"Token:{token}", "Service") ?? throw new InvalidOperationException();
                await _redisCache.RemoveAsync($"Token:{token}", "Service");
                await _notificationService.InvalidateAsync(request.Company, request.Code, () => _infrastructureRepository.GetNotificationsAsync(request.Company, request.Code));
                return Ok();
            }
        }
    }
}