using System;
using System.Threading.Tasks;
using System.Web.Http;
using CheckListManager.Common.Logic.Api;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Instances.Logic.Messages;
using CheckListManager.Instances.Logic.Services;

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
        [Authorize(Roles = "Instances#Create")]
        public async Task<IHttpActionResult> Create(CreateInstanceRequest request)
		{
			try
			{
				var cacheObject = await _instanceManagerService.CreateAsync(Identity.CompanyId, Identity.Code, request.Prototype, request.Version);
				var response = new InstanceResponse
				{
					Code = cacheObject.Code,
                    State = cacheObject.InstanceState,

                    CheckList = _instanceManagerService.GetCheckList(Identity.Code, Identity.HasPermission("Instances", "Create", cacheObject.Department, cacheObject.PrototypeType), cacheObject),

                    Department = cacheObject.Department,
                    DepartmentName = cacheObject.DepartmentName,
                    PrototypeType = cacheObject.Prototype,
                    PrototypeTypeName = cacheObject.PrototypeTypeName,
                    Prototype = cacheObject.Prototype,
                    PrototypeName = cacheObject.PrototypeName,
                    Version = cacheObject.Version,
                    VersionState = cacheObject.VersionState
                };
				return Ok(Message.Create(response));
			}
			catch (Exception e)
			{
				return Error(e);
			}
		}

		[HttpPost]
		[Route("Open")]
        [Authorize(Roles = "Instances#Create")]
        public async Task<IHttpActionResult> Open(InstanceRequest request)
		{
			try
			{
				await _instanceManagerService.OpenAsync(request.Code, Identity);
				return Ok(Message.Create(true));
			}
			catch (Exception e)
			{
				return Error(e);
			}
		}

		[HttpPost]
		[Route("Get")]
        [Authorize(Roles = "Instances#View")]
        public async Task<IHttpActionResult> Get(InstanceRequest request)
		{
			try
			{
				var cacheObject = await _instanceManagerService.GetAsync(Identity.CompanyId, request.Code);
				var response = new InstanceResponse
				{
					Id = cacheObject.Id,
					Code = cacheObject.Code,
                    Year = cacheObject.Year,
                    Number = cacheObject.Number,
                    State = cacheObject.InstanceState,

                    CheckList = _instanceManagerService.GetCheckList(Identity.Code, Identity.HasPermission("Instances", "View", cacheObject.Department, cacheObject.PrototypeType), cacheObject),

                    Department = cacheObject.Department,
                    DepartmentName = cacheObject.DepartmentName,
                    PrototypeType = cacheObject.Prototype,
                    PrototypeTypeName = cacheObject.PrototypeTypeName,
                    Prototype = cacheObject.Prototype,
                    PrototypeName = cacheObject.PrototypeName,
                    Version = cacheObject.Version,
                    VersionState = cacheObject.VersionState
                };
				return Ok(Message.Create(response));
			}
			catch (Exception e)
			{
				return Error(e);
			}
		}

		[HttpPost]
		[Route("Save")]
		[Authorize(Roles = "Instances#Edit")]
		public async Task<IHttpActionResult> Save(InstanceRequest request)
		{
			try
			{
				await _instanceManagerService.SaveAsync(request.Code, Identity);
				return Ok(Message.Create(true));
			}
			catch (Exception e)
			{
				return Error(e);
			}
		}

		[HttpPost]
		[Route("Close")]
        [Authorize(Roles = "Instances#View,Instances#Edit")]
        public async Task<IHttpActionResult> Close(InstanceRequest request)
		{
			try
			{
				await _instanceManagerService.CloseAsync(request.Code, Identity);
				return Ok(Message.Create(true));
			}
			catch (Exception e)
			{
				return Error(e);
			}
		}

		[HttpPost]
		[Route("Event")]
        [Authorize(Roles = "Instances#Edit")]
        public async Task<IHttpActionResult> Event(InstanceEventRequest request)
		{
			try
			{
				var cacheObject = await _instanceManagerService.HandleEventAsync(Identity, request);
				var response = new InstanceResponse
				{
					Id = cacheObject.Id,
					Code = cacheObject.Code,
					CheckList = _instanceManagerService.GetCheckList(Identity.Code, Identity.HasPermission("Instances", "Edit", cacheObject.Department, cacheObject.PrototypeType), cacheObject),
                    State = cacheObject.InstanceState
                };
				return Ok(Message.Create(response));
			}
			catch (Exception e)
			{
				return Error(e);
			}
		}
	}
}