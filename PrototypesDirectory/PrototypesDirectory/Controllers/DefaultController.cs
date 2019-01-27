using Matematici.DataEngine;
using Matematici.DataSource;
using Matematici.ViewModels.Browser;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Matematici.Controllers
{
    [Authorize]
    public class DefaultController : BaseController
    {
        [Route]
        public ActionResult Index()
        {
            return ConditionalView("List");
        }

        [HttpPost]
        [Route("Bind")]
        public ActionResult Bind(PagingRequest<ViewModels.Browser.Request> Request)
        {
            var List = new List(Identity.Company);
            if (Engine.Instance.Companies[Identity.Company].Exception != null)
                throw new Exception(Engine.Instance.Companies[Identity.Company].Exception.Message);
            else
                List.Bind(Request);

            return Json(List);
        }

        [HttpPost]
        [Route("GetData")]
        public ActionResult GetData(PagingRequest<ViewModels.Browser.Request> Request)
        {
            return Json(new List(Identity.Company).GetData(Request));
        }

        [Route("Detail")]
        public ActionResult Detail(string FileName)
        {
            return File(Settings.Provider.Values.Companies[Identity.Company].PrototypesFolder + FileName, "application/pdf");
        }

        [AllowAnonymous]
        [Route("EngineError")]
        public ActionResult EngineError()
        {
            return ConditionalView("Error", new HandleErrorInfo(Engine.Instance.Companies[Identity.Company].Exception, "DefaultController", "List"));
        }

        [AllowAnonymous]
        [Route("Error")]
        public ActionResult Error()
        {
            return ConditionalView();
        }
    }
}