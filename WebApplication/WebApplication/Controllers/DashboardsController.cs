using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web.Mvc;

namespace CheckListManager.WebApplication.Controllers
{
    [RoutePrefix("Dashboards")]
    public class DashboardsController : BaseController
    {
        [Route("InstanceLifeCycle")]
        public ActionResult InstanceLifeCycle()
        {
            return ConditionalView();
        }

        [Route("InstanceOverview")]
        public ActionResult InstanceOverview()
        {
            return ConditionalView();
        }

        [Route("Tasks")]
        public ActionResult Tasks()
        {
            return ConditionalView();
        }
    }
}