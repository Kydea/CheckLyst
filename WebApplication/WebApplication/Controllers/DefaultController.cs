using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication.Controllers
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
        [HttpGet]
        [Route("{*Url}", Order = 2)]
        public ActionResult Loader(string Url)
        {
            return ConditionalView("Loader", Url);
        }

        [Route]
        [HttpPost]
        public ActionResult Dashboard(int? PageVersions, int? PageInstances)
        {
            //var Model = new ViewModels.Dashboard.DashboardModel(db);
            //Model.PrototypesVersions.Set(PageVersions ?? 1);
            //Model.Instances.Set(PageInstances ?? 1);
            return ConditionalView();
        }
    }
}