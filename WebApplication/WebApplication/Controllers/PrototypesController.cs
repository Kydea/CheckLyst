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

        [Route("Detail/{Id?}")]
        public ActionResult Detail(long Id)
        {
            return ConditionalView("Detail", new Prototype { Id = Id, Action = Prototype.Actions.Open });
        }

        [Route("Add/{Id?}")]
        public ActionResult Add(long? Id)
        {
            return ConditionalView("Detail", new Prototype { Id = Id, Action = Prototype.Actions.Add });
        }

        [Route("Clone/{Id?}")]
        public ActionResult Clone(long? Id)
        {
            return ConditionalView("Detail", new Prototype { Id = Id, Action = Prototype.Actions.Clone });
        }

    }
}