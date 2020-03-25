using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using CheckListManager.Common.Core.Network;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api;
using CheckListManager.Common.Logic.Api.Formatters;
using CheckListManager.Common.Logic.Api.Instances;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.Cache;
using CheckListManager.Common.Logic.Queue;
using CheckListManager.Common.Logic.Services;
using CheckListManager.Common.Logic.Trail;
using CheckListManager.Instances.Data;
using CheckListManager.Instances.Logic.Messages;
using CheckListManager.Instances.Logic.PDF;
using CheckListManager.Instances.Logic.Services;

namespace CheckListManager.Instances.Api.Controllers
{
    [RoutePrefix("Instance")]
    public class InstanceController : BaseApiController
    {
        private readonly IInstanceManagerService _instanceManagerService;
        private readonly IRedisCache _redisCache;
        private readonly ICommonRepository _commonRepository;
        private readonly IInstancesRepository _instancesRepository;
        private readonly IConfigurationService _configurationService;
        private readonly ISettingService _settingService;
        private readonly IStorageService _storageService;
        private readonly IQueueService _queueService;
        private readonly IServiceClient _client;

        public InstanceController(IInstanceManagerService instanceManagerService, IRedisCache redisCache, ICommonRepository commonRepository, IInstancesRepository instancesRepository, IConfigurationService configurationService, ISettingService settingService, IStorageService storageService, IQueueService queueService, IServiceClient client)
        {
            _instanceManagerService = instanceManagerService;
            _redisCache = redisCache;
            _commonRepository = commonRepository;
            _instancesRepository = instancesRepository;
            _configurationService = configurationService;
            _settingService = settingService;
            _storageService = storageService;
            _queueService = queueService;
            _client = client;
        }

        [HttpPost]
        [Route("Create")]
        [Authorize(Roles = "Instances#Create, Prototypes#Preview")]
        public async Task<IHttpActionResult> Create(CreateInstanceRequest request)
        {
            try
            {
                var cacheObject = await _instanceManagerService.CreateInstanceAsync(Identity, Identity.Code, request.Prototype, request.Version, request.Parameters);
                var instance = await _instanceManagerService.UpdateInstanceAsync(Identity, cacheObject, null);
                await _instanceManagerService.SaveCacheObjectAsync(cacheObject);
                var response = new InstanceResponse
                {
                    Code = cacheObject.Code,
                    State = cacheObject.InstanceState,
                    Department = cacheObject.Department,
                    DepartmentName = cacheObject.DepartmentName,
                    PrototypeType = cacheObject.PrototypeType,
                    PrototypeTypeName = cacheObject.PrototypeTypeName,
                    Prototype = cacheObject.Prototype,
                    PrototypeName = cacheObject.PrototypeName,
                    PrototypeDescription = cacheObject.PrototypeDescription,
                    Version = cacheObject.Version,
                    VersionState = cacheObject.VersionState,

                    CheckList = instance
                };
                if (!Identity.HasPermission("Trail") && cacheObject.VersionState != VersionState.Draft && cacheObject.VersionState != VersionState.InSimulation) instance.HideExpressions = true;
                return Ok(response);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        [HttpPost]
        [Route("Open")]
        [Authorize(Roles = "Instances#Create, Prototypes#Preview")]
        public async Task<IHttpActionResult> Open(InstanceRequest request)
        {
            try
            {
                var cacheObject = await _instanceManagerService.OpenInstanceAsync(request.Code, Identity);
                var instance = await _instanceManagerService.UpdateInstanceAsync(Identity, cacheObject, null);
                await _instanceManagerService.SaveCacheObjectAsync(cacheObject);

                var response = new InstanceResponse
                {
                    Id = cacheObject.Id,
                    State = cacheObject.InstanceState,
                    Year = cacheObject.Year,
                    Number = cacheObject.Number,

                    CheckList = instance
                };
                if (!Identity.HasPermission("Trail") && cacheObject.VersionState != VersionState.Draft && cacheObject.VersionState != VersionState.InSimulation) instance.HideExpressions = true;
                return Ok(response);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        [HttpPost]
        [Route("Save")]
        [Authorize(Roles = "Instances#Edit, Prototypes#Preview")]
        public async Task<IHttpActionResult> Save(InstanceRequest request)
        {
            try
            {
                await _instanceManagerService.SaveInstanceAsync(request.Code, Identity);
                return Ok();
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        [HttpPost]
        [Route("Delete")]
        [Authorize(Roles = "Instances#Delete, Instances#Edit, Prototypes#Preview")]
        public async Task<IHttpActionResult> Delete(InstanceRequest request)
        {
            try
            {
                await _instanceManagerService.DeleteInstanceAsync(request.Code, Identity, request.Note);
                return Ok();
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        [HttpPost]
        [Route("Complete")]
        [Authorize(Roles = "Instances#Edit, Prototypes#Preview")]
        public async Task<IHttpActionResult> Complete(InstanceRequest request)
        {
            try
            {
                var cacheObject = await _instanceManagerService.CompleteInstanceAsync(request.Code, Identity);
                return Ok(new InstanceResponse
                {
                    State = cacheObject.InstanceState,
                    CheckList = cacheObject.Instance
                });
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        [HttpPost]
        [Route("Get")]
        [Authorize(Roles = "Instances#View, Prototypes#Preview")]
        public async Task<IHttpActionResult> Get(InstanceRequest request)
        {
            try
            {
                var cacheObject = await _instanceManagerService.GetCacheObjectAsync(Identity.CompanyId, request.Code, true);
                _instanceManagerService.CheckViewPermission(Identity, cacheObject);
                await _instanceManagerService.GetInstanceAsync(Identity, cacheObject);
                var instance = await _instanceManagerService.UpdateInstanceAsync(Identity, cacheObject, null);
                await _instanceManagerService.SaveCacheObjectAsync(cacheObject);

                var response = new InstanceResponse
                {
                    Id = cacheObject.Id,
                    Code = cacheObject.Code,
                    Year = cacheObject.Year,
                    Number = cacheObject.Number,
                    State = cacheObject.InstanceState,

                    CheckList = instance,

                    Department = cacheObject.Department,
                    DepartmentName = cacheObject.DepartmentName,
                    PrototypeType = cacheObject.PrototypeType,
                    PrototypeTypeName = cacheObject.PrototypeTypeName,
                    Prototype = cacheObject.Prototype,
                    PrototypeName = cacheObject.PrototypeName,
                    PrototypeDescription = cacheObject.PrototypeDescription,
                    Version = cacheObject.Version,
                    VersionState = cacheObject.VersionState
                };
                if (!Identity.HasPermission("Trail") && cacheObject.VersionState != VersionState.Draft && cacheObject.VersionState != VersionState.InSimulation) instance.HideExpressions = true;
                return Ok(response);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        [HttpPost]
        [Route("Event")]
        [Authorize(Roles = "Instances#View, Prototypes#Preview")]
        public async Task<IHttpActionResult> Event(InstanceEventRequest request)
        {
            try
            {
                var cacheObject = await _instanceManagerService.HandleEventAsync(Identity, request);
                var checkList = await _instanceManagerService.UpdateInstanceAsync(Identity, cacheObject, request.Type == "Signature" || request.Type == "SignatureCheck" ? request.Username : null);
                await _instanceManagerService.SaveCacheObjectAsync(cacheObject);
                var response = new InstanceResponse
                {
                    Id = cacheObject.Id,
                    Code = cacheObject.Code,
                    CheckList = checkList,
                    State = cacheObject.InstanceState
                };
                if (!Identity.HasPermission("Trail") && cacheObject.VersionState != VersionState.Draft && cacheObject.VersionState != VersionState.InSimulation) checkList.HideExpressions = true;
                return Ok(response);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        [HttpPost]
        [Route("History")]
        [Authorize(Roles = "Instances#View, Prototypes#Preview")]
        public async Task<IHttpActionResult> History(InstanceRequest request)
        {
            try
            {
                return Ok(await _instanceManagerService.History(Identity, request.ID.Value));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        [HttpPost]
        [Route("Download")]
        [Authorize(Roles = "Instances#View, Prototypes#Preview")]
        public async Task<IHttpActionResult> Download(InstanceRequest request)
        {
            var cacheObject = await _instanceManagerService.GetCacheObjectAsync(Identity.CompanyId, request.Code, false); 
            _instanceManagerService.CheckViewPermission(Identity, cacheObject);

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
            var Data = await _redisCache.GetAsync<InstanceRequest>($"Token:{Token}", "Download");
            await _redisCache.RemoveAsync($"Token:{Token}", "Download");
            var cacheObject = await _instanceManagerService.GetCacheObjectAsync(Data.CompanyId.Value, Data.Code, true);
            cacheObject.Instance.Update(await _settingService.DecimalSeparatorAsync(Data.CompanyId.Value), (await _settingService.TimeZoneAsync(Data.CompanyId.Value)), cacheObject);
            cacheObject.Instance.UpdateReadOnly(Data.UserCode, cacheObject.InstanceState);
            var PDF = new InstancePDF(Data.CompanyId.Value, Data.CompanyName, Data.TimeZone, _configurationService.PublicRepositoryUrl, cacheObject, _instancesRepository, _storageService, _configurationService);

            await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
            {
                Company = Data.CompanyId.Value,
                User = Data.UserCode,
                Action = TrailAction.InstanceDownloaded,
                Date = DateTimeOffset.UtcNow,
                CheckList = cacheObject.InstanceCode
            });
            
            return Content(HttpStatusCode.OK, PDF, new PDFFormatter(string.IsNullOrEmpty(cacheObject.Instance.FileName) ? "Instance" : cacheObject.Instance.FileName));
        }

        [HttpPost]
        [Route("GetExternalSourceRows")]
        [Authorize(Roles = "Instances#View, Prototypes#Preview")]
        public async Task<IHttpActionResult> GetExternalSourceRows(PagingRequest<ExternalSourceRequest> request)
        {
            try
            {
                return Ok(await _instanceManagerService.GetExternalSourceRows(request.Data.InstanceCode, Identity, request.Data.ExternalSourceCode, request.Data.Parameters, request.Data.Page));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        [HttpPost]
        [Route("GetInstanceEditors")]
        [Authorize(Roles = "Instances#Edit, Prototypes#Preview")]
        public async Task<IHttpActionResult> GetInstanceEditors(InstanceRequest request)
        {
            try
            {
                return Ok(await _instanceManagerService.GetInstanceEditors(request.Code, Identity));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        [HttpGet]
        [Route("DownloadAttachment")]
        [Authorize(Roles = "Instances#View, Prototypes#Preview")]
        public async Task<IHttpActionResult> DownloadAttachment([FromUri] string code, [FromUri]string key)
        {
            try
            {
                var cacheObject = await _instanceManagerService.GetCacheObjectAsync(Identity.CompanyId, code, false);
                _instanceManagerService.CheckViewPermission(Identity, cacheObject);
                var response = await _instanceManagerService.DownloadAttachmentAsync(Identity, cacheObject.Code, key);
                return Content(HttpStatusCode.OK, response.Content, new FileFormatter(response.ContentType));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        [HttpPost]
        [Route("InvalidateCache")]
        [Authorize(Roles = "Prototypes#Edit")]
        public async Task<IHttpActionResult> InvalidateCache(InvalidateCacheRequest request)
        {
            await _instanceManagerService.InvalidateCacheAsync(Identity, request);
            return Ok();
        }
    }
}