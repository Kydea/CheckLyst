using CheckListManager.Common.Logic.Api;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Infrastructure.Logic.Companies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace CheckListManager.Infrastructure.Api.Controllers
{
    [RoutePrefix("Companies")]
    public class CompaniesController : BaseApiController
    {
        private readonly ICompaniesService _companiesService;

        public CompaniesController(ICompaniesService companiesService)
        {
            _companiesService = companiesService;
        }

        [HttpPost]
        [Route("GetData")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> GetData()
        {
            try
            {
                return Ok(await _companiesService.GetDataAsync());
            }
            catch (Exception e)
            {
                return Error(new Exception("Error retrieving the companies list", e));
            }
        }
    }
}
