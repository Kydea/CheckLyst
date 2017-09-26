using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using CheckListManager.Common.Logic.Messages;

namespace CheckListManager.Common.Logic
{
	public class ErrorResult : IHttpActionResult
	{
		private readonly ExceptionHandlerContext _context;
		private readonly HttpRequestMessage _request;
		private ResponseMessage _message;

		public ErrorResult(ExceptionHandlerContext context)
		{
			_context = context;
		}

		public ErrorResult(HttpRequestMessage request, ResponseMessage message)
		{
			_request = request;
			_message = message;
		}

		public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
		{
			if (_message == null)
			{
				_message = new ResponseMessage();
				_message.Messages.Add(_context.Exception?.Message ?? "An error has occurred.");
			}

			var request = _request ?? _context.Request;

			return Task.FromResult(request.CreateResponse(HttpStatusCode.InternalServerError, _message));
		}
	}
}
