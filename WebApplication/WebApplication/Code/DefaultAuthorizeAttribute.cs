using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using CheckListManager.Common.Core.Web.Security;

namespace CheckListManager.WebApplication.Code
{
	public class DefaultAuthorizeAttribute : AuthorizeAttribute
	{
		protected override bool AuthorizeCore(HttpContextBase httpContext)
		{
			var result = base.AuthorizeCore(httpContext);

			if (httpContext.User.Identity.IsAuthenticated)
			{
				httpContext.User =
					new ClaimsPrincipal(new DefaultIdentity((ClaimsIdentity)httpContext.User.Identity));
			}

			return result;
		}
	}
}