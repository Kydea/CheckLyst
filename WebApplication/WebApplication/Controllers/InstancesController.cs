using System.Web.Mvc;
using CheckListManager.WebApplication.Code;
using CheckListManager.WebApplication.Models;

namespace CheckListManager.WebApplication.Controllers
{
    [RoutePrefix("Instances")]
    public class InstancesController : BaseController
	{
        [Route("List")]
        [Authorize(Roles = "FunctionGroup#Instances")]
        public ActionResult List()
        {
            return ConditionalView("List");
        }

        [Route("Create/{Prototype?}/{Version?}")]
        [Authorize(Roles = "FunctionGroup#Instances,Prototypes#Edit")]
        public ActionResult Create(long? Prototype, int? Version)
        {
            return ConditionalView("Detail", new Instance { Prototype = Prototype.Value, Version = Version.Value });
        }

        [Route("Detail/{Code?}")]
        [Authorize(Roles = "FunctionGroup#Instances,Prototypes#Edit")]
        public ActionResult Detail(string Code)
        {
            return ConditionalView("Detail", new Instance { Code = Code });
        }

        [Route("Signature")]
        [Authorize(Roles = "FunctionGroup#Instances,Prototypes#Edit")]
        public ActionResult Signature()
        {
            return ConditionalView("Signature");
        }
    }
}