using System.Threading.Tasks;
using System.Web.Http;
using System;

namespace CheckListManager.ExternalSources.Api.Controllers
{
	public class BaseController: ApiController
    {
        [Route("Test")]
        public async Task<IHttpActionResult> Test()
        {
            var SAPSource = new Logic.SAP.Source();
            SAPSource.Open();
            SAPSource.Test();
            return Ok(); 
        }
    }
}
