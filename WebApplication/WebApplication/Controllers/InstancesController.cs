using System.Web.Mvc;
using CheckListManager.WebApplication.Code;

namespace CheckListManager.WebApplication.Controllers
{
    [RoutePrefix("Instances")]
    [DefaultAuthorize(Roles = "FunctionGroup#Instances")]
    public class InstancesController : BaseController
	{
        [Route("List")]
        public ActionResult List()
        {
            return ConditionalView("List");
        }
    }
}