using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CheckListManager.AccessControl.Controllers
{
	[RoutePrefix("Access")]
	public class AccessController : ApiController
	{
		//[AllowAnonymous]
		//[HttpPost]
		//[Route("Login")]
		//public HttpResponseMessage Login()
		//{
		//	return Request.CreateResponse(HttpStatusCode.OK, "Test anonymous");
		//}

		[AllowAnonymous]
		[HttpGet]
		[Route("Test")]
		public HttpResponseMessage Test()
		{
			return Request.CreateResponse(HttpStatusCode.OK, "Test anonymous");
		}

		[HttpGet]
		[Route("Test2")]
		public HttpResponseMessage Test2()
		{
			return Request.CreateResponse(HttpStatusCode.OK, $"Test authenticated, {User.Identity.IsAuthenticated}, {User.Identity.Name}");
		}
	}
}
