using System.Web.Mvc;

namespace CheckListManager.WebApplication.Controllers
{
    [RoutePrefix("Security")]
    public class SecurityController : BaseController
	{
        [AllowAnonymous]
        [Route("Login")]
        public ActionResult Login(string returnUrl)
        {
            return ConditionalView("Login", returnUrl ?? Url.Action("Dashboard", "Default"));
        }


        [Route("Logout")]
        public ActionResult Logout()
        {
            return Redirect(Url.Action("Logout"));
        }
    }
}