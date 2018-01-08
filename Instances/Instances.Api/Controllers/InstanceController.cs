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

namespace CheckListManager.Instances.Api.Controllers
{
    [RoutePrefix("Instance")]
    public class InstanceController : BaseApiController
    {
        private readonly IInstanceManagerService _instanceManagerService;

        public InstanceController(IInstanceManagerService instanceManagerService)
        {
            _instanceManagerService = instanceManagerService;
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
                    PrototypeType = cacheObject.Prototype,
                    PrototypeTypeName = cacheObject.PrototypeTypeName,
                    Prototype = cacheObject.Prototype,
                    PrototypeName = cacheObject.PrototypeName,
                    PrototypeDescription = cacheObject.PrototypeDescription,
                    Version = cacheObject.Version,
                    VersionState = cacheObject.VersionState,

                    CheckList = _instanceManagerService.GetCheckList(Identity, "Create", cacheObject),
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

                    CheckList = _instanceManagerService.GetCheckList(Identity, "Create", cacheObject),
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
        [Route("Cancel")]
        [Authorize(Roles = "Instances#Delete, Prototypes#Edit")]
        public async Task<IHttpActionResult> Cancel(InstanceRequest request)
        {
            try
            {
                await _instanceManagerService.CancelAsync(request.Code, Identity, request.Note);
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
                var cacheObject = await _instanceManagerService.GetAsync(Identity, request.Code);
                var response = new InstanceResponse
                {
                    Id = cacheObject.Id,
                    Code = cacheObject.Code,
                    Year = cacheObject.Year,
                    Number = cacheObject.Number,
                    State = cacheObject.InstanceState,

                    CheckList = _instanceManagerService.GetCheckList(Identity, "View", cacheObject),

                    Department = cacheObject.Department,
                    DepartmentName = cacheObject.DepartmentName,
                    PrototypeType = cacheObject.Prototype,
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
        [Authorize(Roles = "Instances#Edit, Prototypes#Edit")]
        public async Task<IHttpActionResult> Event(InstanceEventRequest request)
        {
            try
            {
                var cacheObject = await _instanceManagerService.HandleEventAsync(Identity, request);
                var response = new InstanceResponse
                {
                    Id = cacheObject.Id,
                    Code = cacheObject.Code,
                    CheckList = _instanceManagerService.GetCheckList(Identity, "Edit", cacheObject),
                    State = cacheObject.InstanceState
                };
                return Ok(response);
            }
            catch (Exception Exception)
            {
                return Error(Exception);
            }
        }
    }
}