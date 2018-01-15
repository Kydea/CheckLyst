using System.Web.Mvc;
using CheckListManager.WebApplication.Code;

namespace CheckListManager.WebApplication.Controllers
{
    [RoutePrefix("Users")]
    [DefaultAuthorize(Roles = "FunctionGroup#Users")]
    public class UsersController : BaseController
	{
        [Route("List")]
        public ActionResult List()
        {
            return ConditionalView();
        }

        [Route("Detail")]
        public ActionResult Detail()
        {
            return PartialView();
        }
    }
}