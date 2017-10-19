using System.Web.Mvc;
using CheckListManager.WebApplication.Code;

namespace CheckListManager.WebApplication.Controllers
{
    [RoutePrefix("Trail")]
    [DefaultAuthorize(Roles = "FunctionGroup#Trail")]
    public class TrailController : BaseController
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