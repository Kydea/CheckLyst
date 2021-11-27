using CheckListManager.WebApplication.Models;
using System.Web.Mvc;

namespace CheckListManager.WebApplication.Controllers
{
    [RoutePrefix("Security")]
    public class SecurityController : BaseController
    {
        [AllowAnonymous]
        public ActionResult Login()
        {
            return ConditionalView("Login", new Login { Administration = false });
        }

        [AllowAnonymous]
        [Route("Administration")]
        public ActionResult Administration()
        {
            return ConditionalView("Login", new Login { Administration = true });
        }

        [Route("Signature")]
        public ActionResult Signature()
        {
            return ConditionalView("Signature");
        }
    }
}