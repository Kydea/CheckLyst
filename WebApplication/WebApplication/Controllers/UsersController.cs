using System.Web.Mvc;
using CheckListManager.WebApplication.Code;

namespace CheckListManager.WebApplication.Controllers
{
    [RoutePrefix("Users")]
    [DefaultAuthorize(Roles = "FunctionGroup#Users")]
    public class UsersController : BaseController
	{
        [Route("UsersList")]
        public ActionResult UsersList()
        {
            return ConditionalView();
        }

        [Route("UsersDetail")]
        public ActionResult UsersDetail()
        {
            return PartialView();
        }

        [Route("RolesList")]
        public ActionResult RolesList()
        {
            return ConditionalView();
        }

        [Route("RolesDetail")]
        public ActionResult RolesDetail()
        {
            return PartialView();
        }

        [Route("GroupsList")]
        public ActionResult GroupsList()
        {
            return ConditionalView();
        }

        [Route("GroupsDetail")]
        public ActionResult GroupsDetail()
        {
            return PartialView();
        }

        [Route("TimeZone")]
        public ActionResult TimeZone()
        {
            return PartialView();
        }
    }
}