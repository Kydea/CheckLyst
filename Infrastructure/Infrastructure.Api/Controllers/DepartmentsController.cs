using CheckListManager.Common.Logic.Api;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Infrastructure.Logic.Departments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace CheckListManager.Infrastructure.Api.Controllers
{
    [RoutePrefix("Departments")]
    public class DepartmentsController : BaseApiController
    {
        private readonly IDepartmentsService _departmentsService;

        public DepartmentsController(IDepartmentsService departmentsService)
        {
            _departmentsService = departmentsService;
        }

        [HttpPost]
        [Route("GetData")]
        public async Task<IHttpActionResult> GetData(PagingRequest<DepartmentsRequest> request)
        {
            return Ok(await _departmentsService.GetDataAsync(Identity.CompanyId, request));
        }

        [HttpPost]
        [Route("Add")]
        public async Task<IHttpActionResult> Add(Message<DepartmentsRow> request)
        {
            return Ok(await _departmentsService.AddOrEdit(Identity, request.Data, true));
        }

        [HttpPost]
        [Route("Edit")]
        public async Task<IHttpActionResult> Edit(Message<DepartmentsRow> request)
        {
            return Ok(await _departmentsService.AddOrEdit(Identity, request.Data, false));
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(Message<DepartmentsRow> request)
        {
            return Ok(await _departmentsService.Delete(Identity, request.Data.ID.Value));
        }
    }
}
