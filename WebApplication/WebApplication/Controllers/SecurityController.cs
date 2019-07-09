using System.Web.Mvc;

namespace CheckListManager.WebApplication.Controllers
{
    [RoutePrefix("Security")]
    public class SecurityController : BaseController
	{
        [AllowAnonymous]
        public ActionResult Login()
        {
            return ConditionalView("Login");
        }

        [Route("Signature")]
        public ActionResult Signature()
        {
            return ConditionalView("Signature");
        }
    }
}