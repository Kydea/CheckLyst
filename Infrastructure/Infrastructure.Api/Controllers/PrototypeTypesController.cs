using CheckListManager.Common.Logic.Api;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Infrastructure.Logic.PrototypeTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace CheckListManager.Infrastructure.Api.Controllers
{
    [RoutePrefix("PrototypeTypes")]
    public class PrototypeTypesController : BaseApiController
    {
        private readonly IPrototypeTypesService _prototypeTypesService;

        public PrototypeTypesController(IPrototypeTypesService prototypeTypesService)
        {
            _prototypeTypesService = prototypeTypesService;
        }

        [HttpPost]
        [Route("Bind")]
        [Authorize(Roles = "PrototypeTypes#View")]
        public async Task<IHttpActionResult> Bind(Message<PrototypeTypesRequest> request)
        {
            return Ok(await _prototypeTypesService.BindAsync(Identity, request.Data));
        }

        [HttpPost]
        [Route("GetData")]
        [Authorize(Roles = "PrototypeTypes#View")]
        public async Task<IHttpActionResult> GetData(PagingRequest<PrototypeTypesRequest> request)
        {
            return Ok(await _prototypeTypesService.GetDataAsync(Identity.CompanyId, request));
        }

        [HttpPost]
        [Route("GetSyntheticData")]
        [Authorize(Roles = "Prototypes#View, Users#View")]
        public async Task<IHttpActionResult> GetSyntheticData(PagingRequest<PrototypeTypesRequest> request)
        {
            return Ok(await _prototypeTypesService.GetSyntheticDataAsync(Identity.CompanyId, request));
        }

        [HttpPost]
        [Route("Add")]
        [Authorize(Roles = "PrototypeTypes#Edit")]
        public async Task<IHttpActionResult> Add(Message<PrototypeTypesRequest> request)
        {
            return Ok(await _prototypeTypesService.Add(Identity, request.Data));
        }

        [HttpPost]
        [Route("Edit")]
        [Authorize(Roles = "PrototypeTypes#Edit")]
        public async Task<IHttpActionResult> Edit(Message<PrototypeTypesRequest> request)
        {
            return Ok(await _prototypeTypesService.Edit(Identity, request.Data));
        }

        [HttpPost]
        [Route("Delete")]
        [Authorize(Roles = "PrototypeTypes#Edit")]
        public async Task<IHttpActionResult> Delete(Message<PrototypeTypesRequest> request)
        {
            return Ok(await _prototypeTypesService.Delete(Identity, request.Data.ID.Value));
        }

    }
}
