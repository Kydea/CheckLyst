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
    public class DefaultController : BaseController
    {
        [AllowAnonymous]
        [Route("Settings")]
        public ActionResult Settings()
        {
            return Json(new { API = ConfigurationManager.AppSettings["BaseUrl"] }, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        [Route("Error", Order = 1)]
        public ActionResult Error()
        {
            return ConditionalView();
        }

        [AllowAnonymous]
        public ActionResult Loader(string url)
        {
            //Load Balancer Healt Check
            if ("ping".Equals(url, StringComparison.InvariantCultureIgnoreCase))
                return Ping();

            return ConditionalView("Loader", url ?? "Dashboard");
        }

        [Route("Dashboard")]
        public ActionResult Dashboard()
        {
            return ConditionalView();
        }

        [HttpPost]
        [Route]
        public ActionResult RedirectToDashboard()
        {
            return RedirectToAction("Dashboard");
        }

        [Route("LicenseManager")]
        public ActionResult LicenseManager()
        {
            return ConditionalView();
        }

        [Route("NotificationsDiagnostics")]
        public ActionResult NotificationsDiagnostics()
        {
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