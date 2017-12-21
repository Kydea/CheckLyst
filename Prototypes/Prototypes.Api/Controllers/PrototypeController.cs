using System.Threading.Tasks;
using System.Web.Http;
using CheckListManager.Common.Logic.Api;
using CheckListManager.Prototypes.Logic.Messages;
using CheckListManager.Prototypes.Logic.Services;
using System.Linq;
using System;
using CheckListManager.Common.Logic.Api.Messages;

namespace CheckListManager.Prototypes.Api.Controllers
{
    [RoutePrefix("Prototype")]
    public class PrototypeController : BaseApiController
    {
        private readonly IPrototypeService _prototypeService;

        public PrototypeController(IPrototypeService prototypeService)
        {
            _prototypeService = prototypeService;
        }

        [HttpPost]
        [Route("Open")]
        [Authorize(Roles = "Prototypes#View")]
        public async Task<IHttpActionResult> Open(PrototypeRequest request)
        {
            try
            {
                return Ok(await _prototypeService.OpenAsync(Identity, request));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        [HttpPost]
        [Route("Clone")]
        [Authorize(Roles = "Prototypes#Edit")]
        public async Task<IHttpActionResult> Clone(PrototypeRequest request)
        {
            try
            {
                return Ok(await _prototypeService.CloneAsync(Identity, request));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        [HttpPost]
        [Route("Version")]
        [Authorize(Roles = "Prototypes#Edit")]
        public async Task<IHttpActionResult> Version(PrototypeRequest request)
        {
            try
            {
                return Ok(await _prototypeService.VersionAsync(Identity, request));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        [HttpPost]
        [Route("Create")]
        [Authorize(Roles = "Prototypes#Edit")]
        public async Task<IHttpActionResult> Create(PrototypeRequest request)
        {
            try
            {
                return Ok(await _prototypeService.CreateAsync(Identity, request));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        [HttpPost]
        [Route("VerifyExpressions")]
        [Authorize(Roles = "Prototypes#Edit")]
        public IHttpActionResult VerifyExpressions(PrototypeRequest request)
        {
            return Ok(_prototypeService.VerifyExpressions(request));
        }

        [HttpPost]
        [Route("Save")]
        [Authorize(Roles = "Prototypes#Edit")]
        public async Task<IHttpActionResult> Save(PrototypeRequest request)
        {
            try
            {
                return Ok(await _prototypeService.SaveAsync(Identity, request));
            }
            catch (Exception e)
            {
                return Error(e);
            }

        }

        [HttpPost]
        [Route("Delete")]
        [Authorize(Roles = "Prototypes#Edit")]
        public async Task<IHttpActionResult> Delete(PrototypeRequest request)
        {
            try
            {
                await _prototypeService.DeleteAsync(Identity, request);
                return Ok();
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        [HttpPost]
        [Route("SetState")]
        [Authorize(Roles = "Prototypes#Edit")]
        public async Task<IHttpActionResult> SetState(PrototypeRequest request)
        {
            try
            {
                return Ok(await _prototypeService.SetStateAsync(Identity, request));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }
    }
}