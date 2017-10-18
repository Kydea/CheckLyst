using System.Web.Mvc;
using CheckListManager.WebApplication.Code;

namespace CheckListManager.WebApplication.Controllers
{
    [RoutePrefix("Users")]
    public class UsersController : BaseController
	{
        [Route("List")]
        [DefaultAuthorize(Roles = "FunctionGroup#Users")]
        public ActionResult List()
        {
            return ConditionalView("List");
        }
    }
}