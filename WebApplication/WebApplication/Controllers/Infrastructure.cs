using System.Web.Mvc;
using CheckListManager.WebApplication.Code;

namespace CheckListManager.WebApplication.Controllers
{
    [RoutePrefix("Infrastructure")]
    [DefaultAuthorize(Roles = "FunctionGroup#Trail")]
    public class InfrastructureController : BaseController
	{
        [Route("Departments/List")]
        public ActionResult DepartmentsList()
        {
            return ConditionalView("Departments/List");
        }

        [HttpPost]
        [Route("Departments/Detail")]
        public ActionResult DepartmentsDetail()
        {
            return PartialView("Departments/Detail");
        }

        [Route("PrototypeTypes/List")]
        public ActionResult PrototypeTypesList()
        {
            return ConditionalView("PrototypeTypes/List");
        }

        [HttpPost]
        [Route("PrototypeTypes/Detail")]
        public ActionResult PrototypeTypesDetail()
        {
            return PartialView("PrototypeTypes/Detail");
        }
    }
}