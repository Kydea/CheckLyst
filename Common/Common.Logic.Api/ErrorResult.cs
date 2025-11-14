using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using CheckListManager.Common.Logic.Api.Messages;
using Newtonsoft.Json;

namespace CheckListManager.Common.Logic.Api
{
	public class ErrorResult : IHttpActionResult
	{
		private readonly HttpRequestMessage _request;
		private ResponseMessage _message;
		private readonly HttpStatusCode _statusCode;

        public ErrorResult(HttpRequestMessage request, ResponseMessage message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError) 
		{
			_request = request;
			_message = message;
			_statusCode = statusCode;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
		{
            if (_message == null)
			{
				_message = new ResponseMessage();
				_message.Messages.Add("An error has occurred.");
			}
			return Task.FromResult(_request.CreateResponse(_statusCode, _message));
		}
	}
}
