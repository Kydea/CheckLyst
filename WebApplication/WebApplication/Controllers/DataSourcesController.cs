using System.Web.Mvc;
using CheckListManager.WebApplication.Code;

namespace CheckListManager.WebApplication.Controllers
{
    [RoutePrefix("DataSources")]
    [DefaultAuthorize(Roles = "FunctionGroup#DataSources")]
    public class DataSourcesController : BaseController
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