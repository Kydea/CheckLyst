using Matematici.Security;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Matematici.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
        private DefaultIdentity _identity;

        protected DefaultIdentity Identity
            => _identity ?? (_identity = new DefaultIdentity(User?.Identity as ClaimsIdentity));

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
            return Request.IsAjaxRequest() ? (ActionResult)PartialView() : View();
        }

        protected virtual ActionResult ConditionalView(string ViewName)
        {
            CheckAppMode();
            return Request.IsAjaxRequest() ? (ActionResult)PartialView(ViewName) : View(ViewName);
        }

        protected virtual ActionResult ConditionalView(object Model)
        {
            CheckAppMode();
            return Request.IsAjaxRequest() ? (ActionResult)PartialView(Model) : View(Model);
        }

        protected virtual ActionResult ConditionalView(string ViewName, object Model)
        {
            CheckAppMode();
            return Request.IsAjaxRequest() ? (ActionResult)PartialView(ViewName, Model) : View(ViewName, Model);
        }

        protected override JsonResult Json(object Data, string ContentType, Encoding ContentEncoding)
        {
            return Json(Data, ContentType, ContentEncoding, JsonRequestBehavior.DenyGet);
        }

        protected override JsonResult Json(object Data, string ContentType, Encoding ContentEncoding, JsonRequestBehavior JsonRequestBehavior)
        {
            return new ActionResults.JsonResult
            {
                Data = Data,
                ContentType = ContentType,
                ContentEncoding = ContentEncoding,
                JsonRequestBehavior = JsonRequestBehavior
            };
        }

        [System.AttributeUsage(AttributeTargets.Class | System.AttributeTargets.Struct)]
        public class ControllerAuthorization : AuthorizeAttribute
        {
            public bool Authorized = true;
            public string RedirectAction;
            public string RedirectController;
            private AuthType Type;

            public enum AuthType { SettingsManager };

            public ControllerAuthorization(AuthType Type, string RedirectAction, string RedirectController)
            {
                this.Type = Type;
                this.RedirectAction = RedirectAction;
                this.RedirectController = RedirectController;
            }

            protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(
                        new
                        {
                            controller = RedirectController,
                            action = RedirectAction
                        })
                    );
            }
        }
    }
}