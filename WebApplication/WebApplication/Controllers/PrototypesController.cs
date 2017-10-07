using System.Web.Mvc;
using CheckListManager.WebApplication.Code;

namespace CheckListManager.WebApplication.Controllers
{
    [RoutePrefix("Prototypes")]
    public class PrototypesController : BaseController
	{
        [Route("List")]
        [DefaultAuthorize(Roles = "FunctionGroup#Prototype")]
        public ActionResult List()
        {
            return ConditionalView("List");
        }
    }
}