using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace CheckListManager.WebApplication.Code
{
	public class DefaultAuthorizeAttribute : AuthorizeAttribute
	{
		protected override bool AuthorizeCore(HttpContextBase httpContext)
		{
			/*
			var result = base.AuthorizeCore(httpContext);

			if (httpContext.User.Identity.IsAuthenticated)
				httpContext.User = new ClaimsPrincipal(new DefaultIdentity((ClaimsIdentity)httpContext.User.Identity));
			*/
			return true;
		}
	}
}