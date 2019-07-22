using System.Web.Mvc;
using CheckListManager.WebApplication.Code;

namespace CheckListManager.WebApplication.Controllers
{
    [RoutePrefix("FTPServers")]
    [DefaultAuthorize(Roles = "FunctionGroup#FTPServers")]
    public class FTPServersController : BaseController
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