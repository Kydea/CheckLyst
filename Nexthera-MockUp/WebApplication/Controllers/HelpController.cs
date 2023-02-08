using System;
using System.Web.Mvc;

namespace CheckListManager.WebApplication.Controllers
{
    [RoutePrefix("Help")]
    public class HelpController : BaseController
	{
        [HttpPost]
        [Route("{Page?}")]
        [AllowAnonymous]
        public ActionResult Index(string Page)
        {
            return ConditionalView(Page ?? "Index");
        }
    }
}