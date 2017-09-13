using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication.Controllers
{
	public class DefaultController : BaseController
	{
        [Route]
        public ActionResult Dashboard(int? PageVersions, int? PageInstances)
        {
            //var Model = new ViewModels.Dashboard.DashboardModel(db);
            //Model.PrototypesVersions.Set(PageVersions ?? 1);
            //Model.Instances.Set(PageInstances ?? 1);
            return ConditionalView();
        }
    }
}