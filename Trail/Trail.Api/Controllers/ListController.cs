using CheckListManager.Common.Logic.Api;
using CheckListManager.Common.Logic.Api.Messages;
using System.Threading.Tasks;
using System.Web.Http;
using CheckListManager.Trail.Logic.Services;

namespace CheckListManager.Trail.Api.Controllers
{
    [RoutePrefix("List")]
    public class ListController : BaseApiController
    {
        private readonly IListService _listService;

        public ListController(IListService listService)
        {
            _listService = listService;
        }

        [HttpPost]
        [Route("GetData")]
        [Authorize(Roles = "Trail#View")]
        public async Task<IHttpActionResult> GetData(PagingRequest<ListRequest> request)
        {
            return Ok(await _listService.GetDataAsync(Identity.CompanyId, request));
        }
    }
}
