using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using CheckListManager.Common.Logic.Messages;

namespace CheckListManager.Common.Logic.Filters
{
	public class ArgumentNullValidationActionFilterAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuting(HttpActionContext actionContext)
		{
			var list = actionContext.ActionArguments.Where(kv => kv.Value == null).ToList();
			if (list.Count > 0)
			{
				var messages = list.Select(e => $"{e.Key} can't be null");

				var response = new ResponseMessage();
				response.Messages.AddRange(messages);

				actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.BadRequest, response);
			}
		}
	}
}
