using System.Web.Mvc;
using CheckListManager.WebApplication.Code;

namespace CheckListManager.WebApplication.Controllers
{
    [RoutePrefix("Prototypes")]
    [DefaultAuthorize(Roles = "FunctionGroup#Prototypes")]
    public class PrototypesController : BaseController
	{
        [Route("List")]
        public ActionResult List()
        {
            return ConditionalView("List");
        }
    }
}