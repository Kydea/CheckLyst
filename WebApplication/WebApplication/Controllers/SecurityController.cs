using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication.Controllers
{
    [RoutePrefix("Security")]
    public class SecurityController : BaseController
	{
        [AllowAnonymous]
        [Route("Login")]
        public ActionResult Login(String ReturnUrl)
        {
            return ConditionalView(ReturnUrl ?? Url.Action("Dashboard", "Default"));
        }


        [Route("Logout")]
        public ActionResult Logout()
        {
            return Redirect(Url.Action("Logout"));
        }
    }
}