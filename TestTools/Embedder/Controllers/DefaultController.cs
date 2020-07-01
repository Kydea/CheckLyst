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
        public ActionResult Index()
        {
            return View();
        }

        [Route("login")]
        public ActionResult Login()
        {
            return View();
        }
    }
}