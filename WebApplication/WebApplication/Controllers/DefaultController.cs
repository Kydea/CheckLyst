using System;
using System.Web.Mvc;

namespace CheckListManager.WebApplication.Controllers
{
    public class DefaultController : BaseController
	{
        [AllowAnonymous]
        [Route("Error", Order = 1)]
        public ActionResult Error()
        {
            return ConditionalView();
        }

        [AllowAnonymous]
        public ActionResult Loader(string url)
        {
			//Workaround for AWS Load Balancer Healt Check
	        if ("ping".Equals(url, StringComparison.InvariantCultureIgnoreCase))
	        {
		        return Ping();
	        }

            return ConditionalView("Loader", url);
        }

        [Route]
        [HttpPost]
        public ActionResult Dashboard(int? pageVersions, int? pageInstances)
        {
            //var Model = new ViewModels.Dashboard.DashboardModel(db);
            //Model.PrototypesVersions.Set(PageVersions ?? 1);
            //Model.Instances.Set(PageInstances ?? 1);
            return ConditionalView();
        }

		[AllowAnonymous]
		[Route("Ping")]
		public ActionResult Ping()
		{
			return Json(new { ok = true }, JsonRequestBehavior.AllowGet);
		}
	}
}