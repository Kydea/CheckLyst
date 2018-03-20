using System.Threading.Tasks;
using System.Web.Http;
using CheckListManager.Common.Logic.Api;
using CheckListManager.Prototypes.Logic.Messages;
using CheckListManager.Prototypes.Logic.Services;
using System.Linq;
using System;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Data;
using CheckListManager.Prototypes.Data;
using Newtonsoft.Json;
using CheckListManager.Prototypes.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Json;
using System.Net;
using CheckListManager.Prototypes.Logic.PDF;
using CheckListManager.Common.Logic.Api.Formatters;

namespace CheckListManager.Prototypes.Api.Controllers
{
    [RoutePrefix("Prototype")]
    public class PrototypeController : BaseApiController
    {
        private readonly IPrototypeService _prototypeService;
        private readonly IPrototypesRepository _prototypesRepository;
        private readonly CheckListJsonConverter _converter;

        public PrototypeController(IPrototypeService prototypeService, IPrototypesRepository prototypesRepository, CheckListJsonConverter converter)
        {
            _prototypeService = prototypeService;
            _prototypesRepository = prototypesRepository;
            _converter = converter;
        }

        [HttpPost]
        [Route("Open")]
        [Authorize(Roles = "Prototypes#View, Instances#Create")]
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

        [HttpPost]
        [Route("History")]
        [Authorize(Roles = "Prototypes#View")]
        public async Task<IHttpActionResult> History(PrototypeRequest request)
        {
            try
            {
                return Ok(await _prototypeService.History(Identity, request));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }


        //Use to test the pdf generation and comment again when finished
        [HttpGet]
        [Route("Export")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> Export()
        {
            var PrototypeVersion = await _prototypesRepository.GetPrototypeVersionAsync(2, 198, 2);
            var CheckList = JsonConvert.DeserializeObject<Prototype>(PrototypeVersion.Data, _converter);

            var PDF = new PrototypePDF(2, "Janssen", "W. Europe Standard Time", "https://s3-eu-west-1.amazonaws.com/public.matematici-clm.com/", PrototypeVersion, CheckList);
            return Content(HttpStatusCode.OK, PDF, new PDFFormatter("Prototype"));
        }
    }
}