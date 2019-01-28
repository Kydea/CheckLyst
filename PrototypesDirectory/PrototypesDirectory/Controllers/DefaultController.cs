using Matematici.ActionResults;
using Matematici.DataEngine;
using Matematici.DataSource;
using Matematici.ViewModels.Browser;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

            List.Bind(Request);
            return Json(List);
        }

        [HttpPost]
        [Route("GetData")]
        public ActionResult GetData(PagingRequest<ViewModels.Browser.Request> Request)
        {
            var List = new List(Identity.Company);
            if (Engine.Instance.Companies[Identity.Company].Exception != null)
                throw new Exception(Engine.Instance.Companies[Identity.Company].Exception.Message);

            return Json(List.GetData(Request));
        }

        [Route("Detail/{Name}")]
        public ActionResult Detail(string Name)
        {
            var PrototypeFile = Engine.Instance.Companies[Identity.Company].GetPrototype(Name);
            if (PrototypeFile == null)
                return RedirectToAction("Error");
            return new PDFResult(PrototypeFile, false);
        }

        [AllowAnonymous]
        [Route("Error")]
        public ActionResult Error()
        {
            return ConditionalView();
        }
    }
}