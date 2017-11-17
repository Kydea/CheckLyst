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
		private readonly IInstanceService _instanceService;

		public InstanceController(IInstanceService instanceService)
		{
			_instanceService = instanceService;
		}

		[HttpGet]
		[Route("Create/{Prototype}/{Version}")]
		public async Task<IHttpActionResult> Create([FromUri]CreateInstanceRequest request)
		{
			try
			{
				var cacheObject = await _instanceService.CreateAsync(request.Prototype, request.Version);
				var response = new CreateInstanceResponse
				{
					Code = cacheObject.Code,
					CheckList = cacheObject.Instance
				};
				return Ok(response);
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
						return Error(new ResponseMessage("An user with the same code already exists."));
					default:
						return Error(new ResponseMessage(exception.Message));
				}
			}
			catch (Exception e)
			{
				return Error(new ResponseMessage(e.Message));
			}
		}
	}
}