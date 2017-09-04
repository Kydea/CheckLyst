using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using CheckListManager.AccessControl.Models;
using CheckListManager.Common.Logic.Cache;
using CheckListManager.Common.Data;
using System.Collections.Generic;

namespace CheckListManager.AccessControl.Controllers
{
	[RoutePrefix("Access")]
	public class AccessController : ApiController
	{
        [HttpGet]
        [Route("Test")]
        public HttpResponseMessage Test()
        {
            return Request.CreateResponse(HttpStatusCode.OK, TimeZones.Get().Select(x => x.Code));
        }
	}
}
