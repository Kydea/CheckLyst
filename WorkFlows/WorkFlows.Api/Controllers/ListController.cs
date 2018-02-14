using CheckListManager.Common.Logic.Api;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.WorkFlows.Logic.Detail;
using CheckListManager.WorkFlows.Logic.List;
using System.Threading.Tasks;
using System.Web.Http;

namespace CheckListManager.WorkFlows.Api.Controllers
{
    [RoutePrefix("List")]
    public class ListController : BaseApiController
    {
        private readonly IListService _listService;
        private readonly IDetailService _detailService;

        public ListController(IListService listService, IDetailService detailService)
        {
            _listService = listService;
            _detailService = detailService;
        }

        [HttpPost]
        [Route("GetData")]
        [Authorize(Roles = "WorkFlows#View, Prototypes#Edit, Prototypes#View")]
        public async Task<IHttpActionResult> GetData(PagingRequest<ListRequest> request)
        {
            return Ok(await _listService.GetDataAsync(Identity.CompanyId, request));
        }

        [HttpPost]
        [Route("Load")]
        [Authorize(Roles = "WorkFlows#View")]
        public async Task<IHttpActionResult> Get(Message<DetailRequest> request)
        {
            return Ok(await _detailService.LoadAsync(Identity, request.Data));
        }

        [HttpPost]
        [Route("Add")]
        [Authorize(Roles = "WorkFlows#Edit")]
        public async Task<IHttpActionResult> Add(Message<DetailRequest> request)
        {
            return Ok(await _detailService.AddAsync(Identity, request.Data));
        }

        [HttpPost]
        [Route("Edit")]
        [Authorize(Roles = "WorkFlows#Edit")]
        public async Task<IHttpActionResult> Edit(Message<DetailRequest> request)
        {
            return Ok(await _detailService.EditAsync(Identity, request.Data));
        }

        [HttpPost]
        [Route("Delete")]
        [Authorize(Roles = "WorkFlows#Edit")]
        public async Task<IHttpActionResult> Delete(Message<DetailRequest> request)
        {
            return Ok(await _detailService.DeleteAsync(Identity, request.Data.ID.Value));
        }
    }
}
