using CheckListManager.Common.Logic.Api;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Infrastructure.Logic.FTPServers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace CheckListManager.Infrastructure.Api.Controllers
{
    [RoutePrefix("FTPServers")]
    public class FTPServersController : BaseApiController
    {
        private readonly IFTPServersService _ftpServersService;

        public FTPServersController(IFTPServersService ftpServersService)
        {
            _ftpServersService = ftpServersService;
        }

        [HttpPost]
        [Route("GetData")]
        [Authorize(Roles = "FTPServers#View")]
        public async Task<IHttpActionResult> GetData(PagingRequest<FTPServersRequest> request)
        {
            return Ok(await _ftpServersService.GetDataAsync(Identity, request));
        }

        [HttpPost]
        [Route("Add")]
        [Authorize(Roles = "FTPServers#Edit")]
        public async Task<IHttpActionResult> Add(Message<FTPServersRow> request)
        {
            return Ok(await _ftpServersService.AddOrEdit(Identity, request.Data, true));
        }

        [HttpPost]
        [Route("Edit")]
        [Authorize(Roles = "FTPServers#Edit")]
        public async Task<IHttpActionResult> Edit(Message<FTPServersRow> request)
        {
            return Ok(await _ftpServersService.AddOrEdit(Identity, request.Data, false));
        }

        [HttpPost]
        [Route("Delete")]
        [Authorize(Roles = "FTPServers#Edit")]
        public async Task<IHttpActionResult> Delete(Message<FTPServersRow> request)
        {
            return Ok(await _ftpServersService.Delete(Identity, request.Data.ID.Value));
        }
    }
}
