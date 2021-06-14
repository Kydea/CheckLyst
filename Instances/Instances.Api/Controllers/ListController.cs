using System.Threading.Tasks;
using System.Web.Http;
using CheckListManager.Common.Logic.Api;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Instances.Data.Models;
using CheckListManager.Instances.Logic.List;

namespace CheckListManager.Instances.Api.Controllers
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
        [Authorize(Roles = "Instances#View, Prototypes#Preview")]
        public async Task<IHttpActionResult> Bind(Message<InstanceFilters> request)
        {
            return Ok(await _listService.BindAsync(Identity, request.Data));
        }

        [HttpPost]
        [Route("GetData")]
        [Authorize(Roles = "Instances#View, Prototypes#Preview")]
        public async Task<IHttpActionResult> GetData(PagingRequest<InstanceFilters> request)
        {
            return Ok(await _listService.GetDataAsync(Identity, request));
        }
    }
}
