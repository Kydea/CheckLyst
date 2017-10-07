using System.Text;
using System.Web.Mvc;
using CheckListManager.Common.Core.Web.Security;

namespace CheckListManager.WebApplication.Controllers
{
    [SessionState(System.Web.SessionState.SessionStateBehavior.Disabled)]
    [Authorize]
    public class BaseController : Controller
    {
	    private DefaultIdentity _identity;

	    protected DefaultIdentity Identity
		    => _identity ?? (_identity = User?.Identity as DefaultIdentity);

	    // ReSharper disable once MemberCanBePrivate.Global
		protected bool IsAppMode()
        {
            return Request["AppMode"] == "1";
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.Exception is InvalidTokenException)
            {
                filterContext.ExceptionHandled = true;
                Response.StatusCode = 498;
            }
            base.OnException(filterContext);
        }

        private void CheckAppMode()
        {
            ViewBag.AppMode = IsAppMode();
        }

        protected ActionResult ConditionalView()
        {
            CheckAppMode();
            return Request.IsAjaxRequest() ? PartialView() : (ActionResult)View();
        }

        protected ActionResult ConditionalView(string viewName)
        {
            CheckAppMode();
            return Request.IsAjaxRequest() ? PartialView(viewName) : (ActionResult)View(viewName);
        }

        protected ActionResult ConditionalView(object model)
        {
            CheckAppMode();
            return Request.IsAjaxRequest() ? PartialView(model) : (ActionResult)View(model);
        }

        protected ActionResult ConditionalView(string viewName, object model)
        {
            CheckAppMode();
            return Request.IsAjaxRequest() ? PartialView(viewName, model) : (ActionResult)View(viewName, model);
        }

        protected override JsonResult Json(object data, string contentType, Encoding contentEncoding)
        {
            return Json(data, contentType, contentEncoding, JsonRequestBehavior.DenyGet);
        }
    }
}