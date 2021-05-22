using System;
using System.Web.Mvc;
using CheckListManager.WebApplication.Code;
using CheckListManager.WebApplication.Models;

namespace CheckListManager.WebApplication.Controllers
{
    [RoutePrefix("Reports")]
    [DefaultAuthorize(Roles = "FunctionGroup#Reports")]
    public class ReportsController : BaseController
	{
        [Route("List")]
        public ActionResult List()
        {
            return ConditionalView();
        }

        [Route("Editor/{Code?}")]
        public ActionResult Editor(string Code)
        {
            return PartialView(new Report { Code = Code });
        }

        [Route("Viewer/{Code?}")]
        public ActionResult Viewer(string Code)
        {
            if (Code == null) throw new ArgumentException(); //we need to have it not mandatory for the base route in the view
            return PartialView(new Report { Code = Code });
        }
    }
}