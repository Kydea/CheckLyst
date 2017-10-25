using System.Web.Mvc;
using CheckListManager.WebApplication.Code;

namespace CheckListManager.WebApplication.Controllers
{
    [RoutePrefix("WorkFlows")]
    [DefaultAuthorize(Roles = "FunctionGroup#WorkFlows")]
    public class WorkFlowsController : BaseController
	{
        [Route("List")]
        public ActionResult List()
        {
            return ConditionalView();
        }

        [HttpPost]
        [Route("Detail")]
        public ActionResult Detail()
        {
            return PartialView();
        }
    }
}