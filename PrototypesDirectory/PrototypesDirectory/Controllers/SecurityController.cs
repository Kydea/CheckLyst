using Matematici.ViewModels.Membership;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Matematici.Controllers
{
    [RoutePrefix("Security")]
    public class SecurityController : BaseController
    {
        [AllowAnonymous]
        [Route("Login")]
        [HttpPost]
        public ActionResult Login(Login Model)
        {
            try
            {
                Security.Security.Login(Model);
                return Json(new { ok = true });
            }
            catch (Exception Exception)
            {
                return Json(new { ok = false, message = Exception.Message });
            }
        }

        [AllowAnonymous]
        [Route("Login")]
        public ActionResult Login(string ReturnUrl)
        {
            Security.Security.Logout();
            return ConditionalView(new Login { ReturnUrl = ReturnUrl ?? Url.Action("Dashboard", "Default") });
        }

        [AllowAnonymous]
        [Route("Logout")]
        public ActionResult Logout()
        {
            Security.Security.Logout();
            return Redirect(Url.Action("Login"));
        }
    }
}