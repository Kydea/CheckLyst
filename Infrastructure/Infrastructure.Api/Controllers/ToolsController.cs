using CheckListManager.Common.Logic.Api;
using CheckListManager.Infrastructure.Logic.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace CheckListManager.Infrastructure.Api.Controllers
{
    [RoutePrefix("Tools")]
    public class ToolsController : BaseApiController
    {
        [HttpPost]
        [Route("DateTimeOffsetConversion")]
        public IHttpActionResult DateTimeOffsetConversion(DateTimeOffsetConversionRequest request)
        {
            return Ok(DateTimeOffsetConversionService.Process(request));
        }
    }
}
