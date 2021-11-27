using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Embedder.Controllers
{
    public class DefaultController : Controller
    {
        [Route]
        public ActionResult Login()
        {
            return View();
        }

        [Route("search-engine")]
        public ActionResult SearchEngine()
        {
            return View();
        }

        [Route("instance")]
        public ActionResult Instance()
        {
            return View();
        }
    }
}