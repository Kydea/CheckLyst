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

        [Route("Editor/NewVersion/{ID}")]
        public ActionResult NewVersion(long ID)
        {
            return PartialView("Editor", new Report { ID = ID, NewVersion = true });
        }

        [Authorize(Roles = "Reports#Edit")]
        [Route("Editor/{ID?}")]
        public ActionResult Editor(long? ID)
        {
            return PartialView(new Report { ID = ID });
        }

        [Authorize(Roles = "Reports#Edit")]
        [Route("Viewer/{ID?}")]
        public ActionResult Viewer(long? ID)
        {
            if (!ID.HasValue) throw new ArgumentException(); //we need to have it not mandatory for the base route in the views
            return PartialView(new Report { ID = ID });
        }

        [Route("Report/{ID}")]
        public ActionResult Report(long ID)
        {
            return PartialView(new Report { ID = ID });
        }

        [Route("Run/{ID}")]
        public ActionResult Run(long ID)
        {
            return PartialView("Report", new Report { Run = ID });
        }

        [Authorize(Roles = "Reports#Edit")]
        [Route("ColumnEditor")]
        public ActionResult ColumnEditor()
        {
            return ConditionalView("ColumnEditor");
        }

        [Authorize(Roles = "Reports#Edit")]
        [Route("ClauseEditor")]
        public ActionResult ClauseEditor()
        {
            return ConditionalView("ClauseEditor");
        }

        [Authorize(Roles = "Reports#Edit")]
        [Route("GroupsList")]
        public ActionResult GroupsList()
        {
            return ConditionalView("GroupsList");
        }

        [Route("Runs")]
        public ActionResult Runs()
        {
            return ConditionalView();
        }
    }
}