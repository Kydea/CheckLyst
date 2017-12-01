using System.Web.Mvc;
using CheckListManager.WebApplication.Code;
using CheckListManager.WebApplication.Models;

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

        [Route("Detail/{Id?}/{Version?}")]
        public ActionResult Detail(long? Id, int? Version)
        {
            return ConditionalView("Detail", new Prototype { Id = Id ?? -1, Version = Version ?? -1, Action = Prototype.OPEN });
        }

        [Route("Clone/{Id?}/{Version?}")]
        public ActionResult Clone(long? Id, int? Version)
        {
            return ConditionalView("Detail", new Prototype { Id = Id ?? -1, Version = Version ?? -1, Action = Prototype.CLONE });
        }

        [Route("Version/{Id?}/{Version?}")]
        public ActionResult Version(long? Id, int? Version)
        {
            return ConditionalView("Detail", new Prototype { Id = Id ?? -1, Version = Version ?? -1, Action = Prototype.VERSION });
        }

        [Route("Create")]
        public ActionResult Create()
        {
            return ConditionalView("Detail", new Prototype { Action = Prototype.CREATE });
        }

    }
}