using System.Linq;
using System.Web.Http;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Logic.Api.Messages;
using System;
using CheckListManager.Common.Data.Exceptions;

namespace CheckListManager.Common.Logic.Api
{
	public class BaseApiController : ApiController
	{
		private DefaultIdentity _identity;

		protected DefaultIdentity Identity
			=> _identity ?? (_identity = User?.Identity as DefaultIdentity);

		protected IHttpActionResult Error(ResponseMessage message)
		{
			return new ErrorResult(Request, message);
		}

        protected IHttpActionResult Error(Exception exception)
        {
            var responseMessage = new ResponseMessage();

            if (exception is DatabaseAggregateException databaseAggregateException)
                responseMessage.Messages.AddRange(databaseAggregateException.InnerExceptions.Select(ie => ie.Message));
            else
                responseMessage.Messages.Add(exception.Message);
            return new ErrorResult(Request, responseMessage);
        }
    }
}
