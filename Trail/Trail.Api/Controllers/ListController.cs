using CheckListManager.Common.Logic.Api;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Trail.Logic.List;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace CheckListManager.Trail.Api.Controllers
{
    public class ListController : BaseApiController
    {
        private readonly IListService _listService;

        public ListController(IListService listService)
        {
            _listService = listService;
        }

        [HttpPost]
        [Route("GetData")]
        public async Task<IHttpActionResult> GetData(PagingRequest<ListRequest> request)
        {
            return Ok(await _listService.GetDataAsync(Identity.CompanyId, request));
        }
    }
}
