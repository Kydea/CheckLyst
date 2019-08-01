using System;
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
        [Authorize(Roles = "FunctionGroup#Instances,Prototypes#Preview")]
        public ActionResult Create(long? Prototype, int? Version)
        {
            ViewBag.AppRoles = Request["roles"];
            ViewBag.AppTimeZone = Request["timeZone"];
            ViewBag.AppDecimalSeparator = Request["decimalSeparator"];
            ViewBag.AppAccessToken = Request["accessToken"];
            ViewBag.AppRefreshToken = Request["refreshToken"];
            return ConditionalView("Detail", new Instance { Prototype = Prototype.Value, Version = Version });
        }

        [Route("Detail/{Code?}")]
        [Authorize(Roles = "FunctionGroup#Instances,Prototypes#Preview")]
        public ActionResult Detail(string Code)
        {
            ViewBag.AppRoles = Request["roles"];
            ViewBag.AppTimeZone = Request["timeZone"];
            ViewBag.AppDecimalSeparator = Request["decimalSeparator"];
            ViewBag.AppAccessToken = Request["accessToken"];
            ViewBag.AppRefreshToken = Request["refreshToken"];
            return ConditionalView("Detail", new Instance { Code = Code });
        }
    }
}