using System.Web.Mvc;
using CheckListManager.WebApplication.Code;

namespace CheckListManager.WebApplication.Controllers
{
    [RoutePrefix("Infrastructure")]
    [DefaultAuthorize(Roles = "FunctionGroup#Trail")]
    public class InfrastructureController : BaseController
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