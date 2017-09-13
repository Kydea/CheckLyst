using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace WebApplication.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
        public BaseController() : base()
        {
        }

        protected override void Dispose(bool Disposing)
        {
            base.Dispose(Disposing);
        }

        protected bool IsAppMode()
        {
            var AppMode = Request["AppMode"] == "1";
            if (AppMode)
                Session["AppMode"] = true;
            else
                AppMode = Session["AppMode"] as bool? ?? false;
            return AppMode;
        }

        private void CheckAppMode()
        {
            ViewBag.AppMode = IsAppMode();
        }

        protected virtual ActionResult ConditionalView()
        {
            CheckAppMode();
            return Request.IsAjaxRequest() ? (ActionResult)PartialView() : (ActionResult)View();
        }

        protected virtual ActionResult ConditionalView(String ViewName)
        {
            CheckAppMode();
            return Request.IsAjaxRequest() ? (ActionResult)PartialView(ViewName) : (ActionResult)View(ViewName);
        }

        protected virtual ActionResult ConditionalView(Object Model)
        {
            CheckAppMode();
            return Request.IsAjaxRequest() ? (ActionResult)PartialView(Model) : (ActionResult)View(Model);
        }

        protected virtual ActionResult ConditionalView(String ViewName, Object Model)
        {
            CheckAppMode();
            return Request.IsAjaxRequest() ? (ActionResult)PartialView(ViewName, Model) : (ActionResult)View(ViewName, Model);
        }

        protected override JsonResult Json(object Data, string ContentType, Encoding ContentEncoding)
        {
            return Json(Data, ContentType, ContentEncoding, JsonRequestBehavior.DenyGet);
        }
    }
}