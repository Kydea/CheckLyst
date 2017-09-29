using System;
using System.Web.Mvc;

namespace CheckListManager.WebApplication.Controllers
{
    [RoutePrefix("Security")]
    public class SecurityController : BaseController
	{
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            return ConditionalView("Login", returnUrl ?? Url.Action("Dashboard", "Default"));
        }
    }
}