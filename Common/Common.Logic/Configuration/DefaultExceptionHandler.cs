using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using CheckListManager.Common.Logic.Messages;
using NLog;

namespace CheckListManager.Common.Logic.Configuration
{
	public class DefaultExceptionHandler : ExceptionHandler
	{
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

		public override void Handle(ExceptionHandlerContext context)
		{
			Logger.Error(context.Exception, "An unhandled exception has occurred.");
			context.Result = new ErrorResult(context);
		}

		private class ErrorResult : IHttpActionResult
		{
			private readonly ExceptionHandlerContext _context;

			public ErrorResult(ExceptionHandlerContext context)
			{
				_context = context;
			}

			public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
			{
				var response = new ResponseMessage();
				response.Messages.Add(_context.Exception?.Message ?? "An error has occurred.");
				return Task.FromResult(_context.Request.CreateResponse(HttpStatusCode.InternalServerError, response));
			}
		}
	}
}
