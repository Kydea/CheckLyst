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
        public async Task<IHttpActionResult> Bind(Message<PrototypeTypesRequest> request)
        {
            return Ok(await _prototypeTypesService.BindAsync(Identity.CompanyId, request.Data));
        }

        [HttpPost]
        [Route("GetData")]
        public async Task<IHttpActionResult> GetData(PagingRequest<PrototypeTypesRequest> request)
        {
            return Ok(await _prototypeTypesService.GetDataAsync(Identity.CompanyId, request));
        }
    }
}
