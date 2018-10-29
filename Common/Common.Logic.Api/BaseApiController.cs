using System.Linq;
using System.Web.Http;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Logic.Api.Messages;
using System;
using CheckListManager.Common.Data.Exceptions;
using CheckListManager.Common.Data;

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
            var message = new ResponseMessage<Exception>();

            if (exception is DatabaseAggregateException databaseAggregateException)
                message.Messages.AddRange(databaseAggregateException.InnerExceptions.Select(ie => ie.Message));
            else if (exception is DatabaseException databaseException)
                switch (databaseException.Error)
                {
                    case DatabaseError.PrimaryKeyViolation:
                        message.Messages.Add("A record with the same key already exists.");
                        break;
                    default:
                        message.Messages.Add(databaseException.Message);
                        break;
                }
            else
                message.Messages.Add(exception.Message);

            message.Data = exception;

            return new ErrorResult(Request, message);
        }
    }
}
