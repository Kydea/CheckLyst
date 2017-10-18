using CheckListManager.Common.Logic.Api;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Prototypes.Logic.List;
using System.Threading.Tasks;
using System.Web.Http;

namespace CheckListManager.Prototypes.Api.Controllers
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
        [Route("Bind")]
        public async Task<IHttpActionResult> Bind(Message<ListRequest> request)
        {
            return Ok(await _listService.BindAsync(Identity.CompanyId, request.Data));
        }

        [HttpPost]
        [Route("GetData")]
        public async Task<IHttpActionResult> GetData(PagingRequest<ListRequest> request)
        {
            return Ok(await _listService.GetDataAsync(Identity.CompanyId, request));
        }
    }
}
