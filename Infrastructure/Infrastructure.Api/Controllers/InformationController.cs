using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Logic.Api;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Infrastructure.Logic.Companies;
using CheckListManager.Infrastructure.Logic.Information;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace CheckListManager.Infrastructure.Api.Controllers
{
    [RoutePrefix("Information")]
    public class InformationController : BaseApiController
    {
        [HttpPost]
        [Route("Get")]
        public IHttpActionResult Get()
        {
            return Ok(new
            {
                ServerTime = DateTimeOffset.UtcNow,
                Version = new ReleaseInfoMessage()
            });
        }

        [HttpPost]
        [Route("Clock")]
        public IHttpActionResult Clock(ClockRequest request)
        {
            return Ok(new
            {
                ServerTime = DateTimeOffset.UtcNow,
                LocalTime = request.LocalTime.ToLocalTime(request.TimeZone)
            });
        }
    }
}
