using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using CheckListManager.Common.Logic.Messages;

namespace CheckListManager.Common.Logic.Filters
{
	public class ModelValidationActionFilterAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuting(HttpActionContext actionContext)
		{
			if (!actionContext.ModelState.IsValid)
			{
				var messages = actionContext.ModelState.SelectMany(m => m.Value.Errors)
					.Select(e => e.Exception?.Message ?? e.ErrorMessage);

				var response = new ResponseMessage();
				response.Messages.AddRange(messages);

				actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.BadRequest, response);
			}
		}
	}
}
