using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using CheckListManager.Common.Data;
using CheckListManager.Common.Data.Exceptions;
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

		[HttpGet]
		[Route("Create/{Prototype}/{Version}")]
		public async Task<IHttpActionResult> Create([FromUri]CreateInstanceRequest request)
		{
			try
			{
				var cacheObject = await _instanceManagerService.CreateAsync(Identity.CompanyId, Identity.Code, request.Prototype, request.Version);
				var response = new InstanceResponse
				{
					Code = cacheObject.Code,
					CheckList = _instanceManagerService.GetCheckList(Identity.Code, Identity.HasPermission("Instances#Edit", cacheObject.PrototypeType), cacheObject)
				};
				return Ok(Message.Create(response));
			}
			catch (Exception e)
			{
				return Error(new ResponseMessage(e.Message));
			}
		}

		[HttpPost]
		[Route("Open")]
		public async Task<IHttpActionResult> Open([FromBody]Message<InstanceRequest> request)
		{
			try
			{
				await _instanceManagerService.OpenAsync(Identity.Code, request.Data.Code);
				return Ok(Message.Create(true));
			}
			catch (DatabaseAggregateException exception)
			{
				return Error(new ResponseMessage(exception.InnerExceptions.Select(e => e.Message).ToArray()));
			}
			catch (DatabaseException exception)
			{
				// ReSharper disable once SwitchStatementMissingSomeCases
				switch (exception.Error)
				{
					case DatabaseError.PrimaryKeyViolation:
						return Error(new ResponseMessage("An instance with the same code already exists."));
					default:
						return Error(new ResponseMessage(exception.Message));
				}
			}
			catch (Exception exception)
			{
				return Error(new ResponseMessage(exception.Message));
			}
		}

		[HttpPost]
		[Route("Get")]
		public async Task<IHttpActionResult> Get([FromBody]Message<InstanceRequest> request)
		{
			try
			{
				var cacheObject = await _instanceManagerService.GetAsync(Identity.CompanyId, request.Data.Code);
				var response = new InstanceResponse
				{
					Id = cacheObject.Id,
					Code = cacheObject.Code,
					CheckList = _instanceManagerService.GetCheckList(Identity.Code, Identity.HasPermission("Instances#Edit", cacheObject.PrototypeType), cacheObject)
				};
				return Ok(Message.Create(response));
			}
			catch (DatabaseAggregateException exception)
			{
				return Error(new ResponseMessage(exception.InnerExceptions.Select(e => e.Message).ToArray()));
			}
			catch (DatabaseException exception)
			{
				return Error(new ResponseMessage(exception.Message));
			}
			catch (Exception exception)
			{
				return Error(new ResponseMessage(exception.Message));
			}
		}

		[HttpPost]
		[Route("Close")]
		public async Task<IHttpActionResult> Close([FromBody]Message<InstanceRequest> request)
		{
			try
			{
				await _instanceManagerService.CloseAsync(Identity.CompanyId, Identity.Code, request.Data.Code);
				return Ok(Message.Create(true));
			}
			catch (DatabaseAggregateException exception)
			{
				return Error(new ResponseMessage(exception.InnerExceptions.Select(e => e.Message).ToArray()));
			}
			catch (DatabaseException exception)
			{
				return Error(new ResponseMessage(exception.Message));
			}
			catch (Exception exception)
			{
				return Error(new ResponseMessage(exception.Message));
			}
		}

		[HttpPost]
		[Route("Event")]
		public async Task<IHttpActionResult> Event([FromBody]Message<InstanceEventRequest> request)
		{
			try
			{
				var cacheObject = await _instanceManagerService.HandleEventAsync(request.Data);
				var response = new InstanceResponse
				{
					Id = cacheObject.Id,
					Code = cacheObject.Code,
					CheckList = _instanceManagerService.GetCheckList(Identity.Code, Identity.HasPermission("Instances#Edit", cacheObject.PrototypeType), cacheObject)
				};
				return Ok(Message.Create(response));
			}
			catch (Exception exception)
			{
				return Error(new ResponseMessage(exception.Message));
			}
		}
	}
}