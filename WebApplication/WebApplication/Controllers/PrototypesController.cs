using System;
using System.Web.Mvc;

namespace CheckListManager.WebApplication.Controllers
{
    [RoutePrefix("Prototypes")]
    public class PrototypesController : BaseController
	{
        [Route("List")]
        [Authorize(Roles = "FunctionGroup#Prototype")]
        public ActionResult List()
        {
            return ConditionalView("List");
        }
    }
}