using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Filters;

namespace CheckListManager.WebApplication.Code
{
	public class AuthenticationFilter : ActionFilterAttribute, IAuthenticationFilter
	{
		public void OnAuthorization(AuthorizationContext filterContext)
		{
			throw new NotImplementedException();
		}

		public void OnAuthentication(AuthenticationContext filterContext)
		{
			throw new NotImplementedException();
		}

		public void OnAuthenticationChallenge(AuthenticationChallengeContext filterContext)
		{
			throw new NotImplementedException();
		}
	}
}