using System.Threading.Tasks;
using System.Web.Http;
using CheckListManager.Common.Logic.Api;
using CheckListManager.Prototypes.Logic.Messages;
using CheckListManager.Prototypes.Logic.Services;
using System;
using CheckListManager.Common.Data;
using CheckListManager.Prototypes.Data;
using Newtonsoft.Json;
using CheckListManager.Prototypes.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Json;
using System.Net;
using System.Web.UI.WebControls;
using CheckListManager.Prototypes.Logic.PDF;
using CheckListManager.Common.Logic.Api.Formatters;
using CheckListManager.Common.Logic.Cache;
using CheckListManager.Common.Logic.Services;

namespace CheckListManager.Prototypes.Api.Controllers
{
    [RoutePrefix("Prototype")]
    public class PrototypeController : BaseApiController
    {
        private readonly IPrototypeService _prototypeService;
        private readonly IPrototypesRepository _prototypesRepository;
        private readonly CheckListJsonConverter _converter;
        private readonly IRedisCache _redisCache;
        private readonly ICommonRepository _commonRepository;
        private readonly IConfigurationService _configurationService;

        public PrototypeController(IPrototypeService prototypeService, IPrototypesRepository prototypesRepository, CheckListJsonConverter converter, IRedisCache redisCache, ICommonRepository commonRepository, IConfigurationService configurationService)
        {
            _prototypeService = prototypeService;
            _prototypesRepository = prototypesRepository;
            _converter = converter;
            _redisCache = redisCache;
            _commonRepository = commonRepository;
            _configurationService = configurationService;
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
        [Route("CheckNewVersion")]
        [Authorize(Roles = "Prototypes#Edit")]
        public async Task<IHttpActionResult> CheckNewVersion(PrototypeRequest request)
        {
            try
            {
                await _prototypeService.CheckNewVersionAsync(Identity, request);
                return Ok();
            }
            catch (Exception e)
            {
                return Ok(new { error = e.Message });
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
        [Route("Upload")]
        [Authorize(Roles = "Prototypes#Edit")]
        public async Task<IHttpActionResult> Upload(UploadRequest request)
        {
	        try
	        {
		        return Ok(await _prototypeService.UploadAsync(Identity, request));
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

        [HttpPost]
        [Route("Download")]
        [Authorize(Roles = "Prototypes#View")]
        public async Task<IHttpActionResult> Download(PrototypeRequest request)
        {
            var Token = Identity.Code + ":" + Guid.NewGuid();
            var Company = await _commonRepository.GetCompanyAsync(Identity.CompanyId);
            request.CompanyId = Identity.CompanyId;
            request.CompanyName = Company.Name;
            request.TimeZone = Company.TimeZones.MicrosoftCode;
            request.UserCode = Identity.Code;
            await _redisCache.SetAsync($"Token:{Token}", request, "Download", new TimeSpan(0, 1, 0));
            return Ok(new { Token });
        }

        [HttpGet]
        [Route("Download")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> Download([FromUri]string Token)
        {
            var Data = await _redisCache.GetAsync<PrototypeRequest>($"Token:{Token}", "Download");
            await _redisCache.RemoveAsync($"Token:{Token}", "Download");
            var PrototypeVersion = await _prototypesRepository.GetPrototypeVersionAsync(Data.CompanyId.Value, Data.ID, Data.Version);
            var CheckList = JsonConvert.DeserializeObject<Prototype>(PrototypeVersion.Data, _converter);

            var PDF = new PrototypePDF(Data.CompanyId.Value, Data.CompanyName, Data.TimeZone, _configurationService.PublicRepositoryUrl, PrototypeVersion, CheckList);
            return Content(HttpStatusCode.OK, PDF, new PDFFormatter("Prototype"));
        }
    }
}