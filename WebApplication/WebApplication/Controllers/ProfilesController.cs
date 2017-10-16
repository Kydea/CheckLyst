using System.Web.Mvc;
using CheckListManager.WebApplication.Code;

namespace CheckListManager.WebApplication.Controllers
{
    [RoutePrefix("Profiles")]
    public class ProfilesController : BaseController
	{
        [Route("List")]
        [DefaultAuthorize(Roles = "FunctionGroup#Profiles")]
        public ActionResult List()
        {
            return ConditionalView("List");
        }
    }
}