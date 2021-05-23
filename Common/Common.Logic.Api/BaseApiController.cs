using System.Linq;
using System.Web.Http;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Logic.Api.Messages;
using System;
using CheckListManager.Common.Data.Exceptions;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Filters;
using System.Collections.Generic;

namespace CheckListManager.Common.Logic.Api
{
    [Compression]
    public class BaseApiController : ApiController
	{
		private DefaultIdentity _identity;

		protected DefaultIdentity Identity
			=> _identity ?? (_identity = User?.Identity as DefaultIdentity);

		protected IHttpActionResult Error(ResponseMessage message)
		{
			return new ErrorResult(Request, message);
		}

        protected List<string> BuildErrorMessage(Exception exception)
        {
            List<string> result = new List<string>();
            if (exception is DatabaseAggregateException databaseAggregateException)
                result.AddRange(databaseAggregateException.InnerExceptions.Select(ie => ie.Message));
            else if (exception is DatabaseException databaseException)
                switch (databaseException.Error)
                {
                    case DatabaseError.PrimaryKeyViolation:
                        result.Add("A record with the same key already exists.");
                        break;
                    default:
                        result.Add(databaseException.Message);
                        break;
                }
            else
                result.Add(exception.Message);
            return result;
        }

        protected IHttpActionResult Error(Exception exception)
        {
            var message = new ResponseMessage<Exception>();
            message.Messages.AddRange(BuildErrorMessage(exception));
            message.Data = exception;

            return new ErrorResult(Request, message);
        }
    }
}
