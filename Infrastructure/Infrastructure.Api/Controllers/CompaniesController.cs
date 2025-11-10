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
        [Route("GetSyntheticData")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> GetSyntheticData()
        {
            try
            {
                return Ok(await _companiesService.GetSyntheticDataAsync());
            }
            catch (Exception e)
            {
                return Error(new Exception("Error retrieving the companies list", e));
            }
        }

        [HttpGet]
        [Route("GetSettings")]
        public async Task<IHttpActionResult> GetSettings()
        {
            try
            {
                return Ok(await _companiesService.GetSettingsAsync(Identity.CompanyId));
            }
            catch (Exception e)
            {
                return Error(new Exception("Error retrieving the company settings", e));
            }
        }

        [HttpPost]
        [Route("Bind")]
        [Authorize(Roles = "Companies#View")]
        public async Task<IHttpActionResult> Bind(Message<CompaniesRequest> request)
        {
            return Ok(await _companiesService.BindAsync(Identity, request.Data));
        }

        [HttpPost]
        [Route("GetData")]
        [Authorize(Roles = "Companies#View")]
        public async Task<IHttpActionResult> GetData(PagingRequest<CompaniesRequest> request)
        {
            return Ok(await _companiesService.GetDataAsync(Identity, request));
        }

        [HttpPost]
        [Route("Add")]
        [Authorize(Roles = "Companies#Edit")]
        public async Task<IHttpActionResult> Add(Message<CompaniesRequest> request)
        {
            return Ok(await _companiesService.AddAsync(Identity, request.Data));
        }

        [HttpPost]
        [Route("Edit")]
        [Authorize(Roles = "Companies#Edit")]
        public async Task<IHttpActionResult> Edit(Message<CompaniesRequest> request)
        {
            return Ok(await _companiesService.EditAsync(Identity, request.Data));
        }

        [HttpPost]
        [Route("Load")]
        [Authorize(Roles = "Companies#View")]
        public async Task<IHttpActionResult> Load(Message<CompaniesRequest> request)
        {
            return Ok(await _companiesService.LoadAsync(Identity, request.Data));
        }

        [HttpPost]
        [Route("SetOnline")]
        [Authorize(Roles = "Companies#Edit")]
        public async Task<IHttpActionResult> SetOnline(Message<CompaniesRequest> request)
        {
            return Ok(await _companiesService.SetOnlineAsync(Identity, request.Data.ID.Value, request.Data.Reason));
        }
    }
}
