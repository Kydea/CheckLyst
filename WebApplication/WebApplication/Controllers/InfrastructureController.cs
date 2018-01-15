using System.Web.Mvc;
using CheckListManager.WebApplication.Code;

namespace CheckListManager.WebApplication.Controllers
{
    [RoutePrefix("Infrastructure")]
    public class InfrastructureController : BaseController
	{
        [DefaultAuthorize(Roles = "FunctionGroup#Departments")]
        [Route("Departments/List")]
        public ActionResult DepartmentsList()
        {
            return ConditionalView("Departments/List");
        }

        [DefaultAuthorize(Roles = "FunctionGroup#Departments")]
        [Route("Departments/Detail")]
        public ActionResult DepartmentsDetail()
        {
            return PartialView("Departments/Detail");
        }

        [Route("PrototypeTypes/List")]
        [DefaultAuthorize(Roles = "FunctionGroup#PrototypeTypes")]
        public ActionResult PrototypeTypesList()
        {
            return ConditionalView("PrototypeTypes/List");
        }

        [Route("PrototypeTypes/Detail")]
        [DefaultAuthorize(Roles = "FunctionGroup#PrototypeTypes")]
        public ActionResult PrototypeTypesDetail()
        {
            return PartialView("PrototypeTypes/Detail");
        }
    }
}