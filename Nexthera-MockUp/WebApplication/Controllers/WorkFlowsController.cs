using System.Web.Mvc;
using CheckListManager.WebApplication.Code;

namespace CheckListManager.WebApplication.Controllers
{
    [RoutePrefix("WorkFlows")]
    public class WorkFlowsController : BaseController
	{
        [Route("List")]
        [DefaultAuthorize(Roles = "FunctionGroup#WorkFlows")]
        public ActionResult List()
        {
            return ConditionalView();
        }

        [Route("Detail")]
        [DefaultAuthorize(Roles = "FunctionGroup#WorkFlows")]
        public ActionResult Detail()
        {
            return PartialView();
        }

        [Route("Signature")]
        public ActionResult Signature()
        {
            return PartialView();
        }
    }
}