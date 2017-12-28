using System.Web.Mvc;
using CheckListManager.WebApplication.Code;
using CheckListManager.WebApplication.Models;

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

        [Route("Create/{Prototype?}/{Version?}")]
        public ActionResult Create(long? Prototype, int? Version)
        {
            return ConditionalView("Detail", new Instance { Prototype = Prototype.Value, Version = Version.Value });
        }

        [Route("Detail/{Code?}")]
        public ActionResult Detail(string Code)
        {
            return ConditionalView("Detail", new Instance { Code = Code });
        }
    }
}