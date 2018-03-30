using System.Web.Http;
using CheckListManager.Common.Logic.Api.Messages;

namespace CheckListManager.Common.Logic.Api.Controllers
{
	[AllowAnonymous]
	[RoutePrefix("ReleaseInfo")]
	public class ReleaseInfoController : BaseApiController
	{
		[Route]
		public IHttpActionResult Get()
		{
            var Model = new ReleaseInfoMessage();
            return Ok(new { Model.Version, Model.BuildDate });
		}
	}
}
