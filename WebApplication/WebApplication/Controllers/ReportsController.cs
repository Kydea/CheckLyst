using System.Web.Mvc;
using CheckListManager.WebApplication.Code;

namespace CheckListManager.WebApplication.Controllers
{
    [RoutePrefix("Reports")]
    [DefaultAuthorize(Roles = "FunctionGroup#Reports")]
    public class ReportsController : BaseController
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