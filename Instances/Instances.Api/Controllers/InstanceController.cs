using System;
using System.Threading.Tasks;
using System.Web.Http;
using CheckListManager.Common.Logic.Api;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Instances.Logic.Messages;
using CheckListManager.Instances.Logic.Services;
using CheckListManager.Instances.Logic.Exceptions;
using CheckListManager.Common.Data;
using CheckListManager.Common.Core.Helpers;
using CheckListManager.Instances.Logic.Models;
using CheckListManager.Common.Logic.Cache;
using System.Net;
using CheckListManager.Common.Logic.Api.Formatters;
using CheckListManager.Common.Logic.Api.PDF;
using CheckListManager.Instances.Logic.PDF;
using CheckListManager.Common.Logic.Services;
using System.Collections.Generic;
using CheckListManager.Common.Core.Network;

namespace CheckListManager.Instances.Api.Controllers
{
    [RoutePrefix("Instance")]
    public class InstanceController : BaseApiController
    {
        private readonly IInstanceManagerService _instanceManagerService;
        private readonly IRedisCache _redisCache;
        private readonly ICommonRepository _commonRepository;
        private readonly IConfigurationService _configurationService;
        private readonly ISettingService _settingService;
        private readonly IServiceClient _client;

        public InstanceController(IInstanceManagerService instanceManagerService, IRedisCache redisCache, ICommonRepository commonRepository, IConfigurationService configurationService, ISettingService settingService, IServiceClient client)
        {
            _instanceManagerService = instanceManagerService;
            _redisCache = redisCache;
            _commonRepository = commonRepository;
            _configurationService = configurationService;
            _settingService = settingService;
            _client = client;
        }

        [HttpPost]
        [Route("Create")]
        [Authorize(Roles = "Instances#Create, Prototypes#Edit")]
        public async Task<IHttpActionResult> Create(CreateInstanceRequest request)
        {
            try
            {
                var cacheObject = await _instanceManagerService.CreateAsync(Identity, Identity.Code, request.Prototype, request.Version);
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

                    CheckList = await _instanceManagerService.GetCheckListAsync(Identity, cacheObject, null),
                };

                return Ok(response);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        [HttpPost]
        [Route("Open")]
        [Authorize(Roles = "Instances#Create, Prototypes#Edit")]
        public async Task<IHttpActionResult> Open(InstanceRequest request)
        {
            try
            {
                var cacheObject = await _instanceManagerService.OpenAsync(request.Code, Identity);
                return Ok(new InstanceResponse
                {
                    Id = cacheObject.Id,
                    State = cacheObject.InstanceState,
                    Year = cacheObject.Year,
                    Number = cacheObject.Number,

                    CheckList = await _instanceManagerService.GetCheckListAsync(Identity, cacheObject, null),
                });
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        [HttpPost]
        [Route("Save")]
        [Authorize(Roles = "Instances#Edit, Prototypes#Edit")]
        public async Task<IHttpActionResult> Save(InstanceRequest request)
        {
            try
            {
                await _instanceManagerService.SaveAsync(request.Code, Identity);
                return Ok();
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        [HttpPost]
        [Route("Delete")]
        [Authorize(Roles = "Instances#Delete, Instances#Edit, Prototypes#Edit")]
        public async Task<IHttpActionResult> Delete(InstanceRequest request)
        {
            try
            {
                await _instanceManagerService.DeleteAsync(request.Code, Identity, request.Note);
                return Ok();
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        [HttpPost]
        [Route("Complete")]
        [Authorize(Roles = "Instances#Edit, Prototypes#Edit")]
        public async Task<IHttpActionResult> Complete(InstanceRequest request)
        {
            try
            {
                var cacheObject = await _instanceManagerService.CompleteAsync(request.Code, Identity);
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
        [Authorize(Roles = "Instances#View, Prototypes#Edit")]
        public async Task<IHttpActionResult> Get(InstanceRequest request)
        {
            try
            {
                var cacheObject = await _instanceManagerService.Open(Identity, request.Code);
                var response = new InstanceResponse
                {
                    Id = cacheObject.Id,
                    Code = cacheObject.Code,
                    Year = cacheObject.Year,
                    Number = cacheObject.Number,
                    State = cacheObject.InstanceState,

                    CheckList = await _instanceManagerService.GetCheckListAsync(Identity, cacheObject, null),

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
                return Ok(response);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        [HttpPost]
        [Route("Event")]
        [Authorize(Roles = "Instances#View, Prototypes#Edit")]
        public async Task<IHttpActionResult> Event(InstanceEventRequest request)
        {
            try
            {
                var cacheObject = await _instanceManagerService.HandleEventAsync(Identity, request);
                var response = new InstanceResponse
                {
                    Id = cacheObject.Id,
                    Code = cacheObject.Code,
                    CheckList = await _instanceManagerService.GetCheckListAsync(Identity, cacheObject, request.Type == "Signature" || request.Type == "SignatureCheck" ? request.Username : null),
                    State = cacheObject.InstanceState
                };
                return Ok(response);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        [HttpPost]
        [Route("History")]
        [Authorize(Roles = "Instances#View, Prototypes#Edit")]
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
        [Route("Export")]
        [Authorize(Roles = "Instances#View, Prototypes#Edit")]
        public async Task<IHttpActionResult> Export(InstanceRequest request)
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
        [Route("Export")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> Export([FromUri]string Token)
        {
            var Data = await _redisCache.GetAsync<InstanceRequest>($"Token:{Token}", "Download");
            await _redisCache.RemoveAsync($"Token:{Token}", "Download");
            var cacheObject = await _instanceManagerService.GetAsync(Data.CompanyId.Value, Data.UserCode, Data.Code);
            cacheObject.Instance.Update(await _settingService.DecimalSeparatorAsync(Data.CompanyId.Value), cacheObject);
            return Content(HttpStatusCode.OK, new InstancePDF(Data.CompanyId.Value, Data.CompanyName, Data.TimeZone, _configurationService.PublicRepositoryUrl, cacheObject), new PDFFormatter(string.IsNullOrEmpty(cacheObject.Instance.FileName) ? "Instance" : cacheObject.Instance.FileName));
        }

        [HttpPost]
        [Route("GetExternalSourceRows")]
        [Authorize(Roles = "Instances#View, Prototypes#Edit")]
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
    }
}