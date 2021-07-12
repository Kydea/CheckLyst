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
using CheckListManager.Common.Core.Helpers;
using CheckListManager.Prototypes.Logic.PDF;
using CheckListManager.Common.Logic.Api.Formatters;
using CheckListManager.Common.Logic.Cache;
using CheckListManager.Common.Logic.Services;
using CheckListManager.Common.Logic.Queue;
using CheckListManager.Common.Logic.Trail;
using System.Linq;

namespace CheckListManager.Prototypes.Api.Controllers
{
    [RoutePrefix("Prototype")]
    public class PrototypeController : BaseApiController
    {
        private readonly IPrototypeService _prototypeService;
        private readonly IPrototypesRepository _prototypesRepository;
        private readonly JsonConverter[] _converters;
        private readonly IRedisCache _redisCache;
        private readonly ICommonRepository _commonRepository;
        private readonly IConfigurationService _configurationService;
        private readonly IStorageService _storageService;
        private readonly ISettingService _settingService;
        private readonly IQueueService _queueService;

        public PrototypeController(
            IPrototypeService prototypeService,
            IPrototypesRepository prototypesRepository,
            CustomJsonConverter[] converters,
            IRedisCache redisCache,
            ICommonRepository commonRepository,
            IConfigurationService configurationService,
            IStorageService storageService,
            ISettingService settingService,
            IQueueService queueService
        )
        {
            _prototypeService = prototypeService;
            _prototypesRepository = prototypesRepository;
            _converters = converters.ToArray<JsonConverter>();
            _redisCache = redisCache;
            _commonRepository = commonRepository;
            _configurationService = configurationService;
            _storageService = storageService;
            _settingService = settingService;
            _queueService = queueService;
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
        [Route("Verify")]
        [Authorize(Roles = "Prototypes#Edit")]
        public IHttpActionResult Verify(PrototypeRequest request)
        {
            try
            {
                request.Parse(_converters);
                return Ok(_prototypeService.Verify(request));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        [HttpPost]
        [Route("Save")]
        [Authorize(Roles = "Prototypes#Edit")]
        public async Task<IHttpActionResult> Save(PrototypeRequest request)
        {
            try
            {
                request.Parse(_converters);
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
        [Authorize(Roles = "Prototypes#Edit, Prototypes#Preview")]
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

        [HttpPost]
        [Route("Download")]
        [Authorize(Roles = "Prototypes#View")]
        public async Task<IHttpActionResult> Download(PrototypeRequest request)
        {
            var Token = Identity.Code + ":" + Guid.NewGuid();
            var Company = await _commonRepository.GetCompanyAsync(Identity.CompanyId);
            request.CompanyId = Identity.CompanyId;
            request.CompanyName = Company.Name;
            request.TimeZone = Company.TimeZone;
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
            var CheckList = JsonConvert.DeserializeObject<Prototype>(PrototypeVersion.Data, _converters);
            CheckList.Setup(false);

            var PDF = new PrototypePDF(
                Data.CompanyId.Value,
                Data.CompanyName,
                Data.TimeZone,
                _configurationService.PublicRepositoryUrl,
                PrototypeVersion,
                CheckList,
                _storageService,
                _configurationService
            );

            await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
            {
                Company = Data.CompanyId.Value,
                User = Data.UserCode,
                Action = TrailAction.PrototypeDownloaded,
                Date = DateTimeOffset.UtcNow,
                CheckListCode = $"{PrototypeVersion.Prototypes.Name}.{PrototypeVersion.Version}",
                CheckListID = PrototypeVersion.Prototype
            });

            return Content(HttpStatusCode.OK, PDF, new PDFFormatter("Prototype"));
        }

        #region Attachment

        [HttpPost]
        [Route("UploadAttachment")]
        [Authorize(Roles = "Prototypes#Edit")]
        public async Task<IHttpActionResult> UploadAttachment(UploadAttachmentRequest request)
        {
            try
            {
                if (request.Id < 0 || request.Version < 0)
                    throw new Exception("Prototype not saved");

                if (request.Attachment == null)
                    throw new Exception("No attachment has been uploaded");

                if (!request.Attachment.MediaType.StartsWith("image/"))
                    throw new Exception("The file uploaded is not supported");

                var maxSize = await _settingService.PrototypesAttachmentMaxSizeBytesAsync(Identity.CompanyId);
                if (request.Attachment.Buffer.Length > maxSize)
                    throw new Exception($"The file exceed the file size limit of {maxSize.BytesCountToString()}");

                return Ok(await _prototypeService.UploadAttachmentAsync(Identity, request));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        [HttpGet]
        [Route("DownloadAttachment")]
        [Authorize(Roles = "Prototypes#View, Instances#View")]
        public async Task<IHttpActionResult> DownloadAttachment([FromUri]ProcessAttachmentRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Key))
                    throw new Exception("Key must have a value");

                var response = await _prototypeService.DownloadAttachmentAsync(Identity, request);
                return Content(HttpStatusCode.OK, response.Content, new FileFormatter(response.ContentType, request.FormatAsBase64));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        #endregion
    }
}