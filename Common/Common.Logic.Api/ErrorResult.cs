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
				_message.Messages.Add("An error has occurred.");
			}

			return Task.FromResult(_request.CreateResponse(HttpStatusCode.InternalServerError, _message));
		}
	}
}
