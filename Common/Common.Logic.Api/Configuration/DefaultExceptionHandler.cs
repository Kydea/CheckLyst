using System;
using System.Collections.Generic;
using System.Web.Http.ExceptionHandling;
using CheckListManager.Common.Logic.License;
using NLog;
using System.Linq;
using System.Data.Entity.Validation;
using CheckListManager.Common.Core.Exceptions;

namespace CheckListManager.Common.Logic.Api.Configuration
{
    public class DefaultExceptionHandler : ExceptionHandler
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private List<string> BuildErrorMessage(Exception exception)
        {
            List<string> result = new List<string>();
            if (exception is AggregateException aggregateException)
                result.AddRange(aggregateException.InnerExceptions.SelectMany(e => BuildErrorMessage(e)));
            else if (exception is DbEntityValidationException dbEntityValidationException)
                result.AddRange(dbEntityValidationException.EntityValidationErrors.SelectMany(v => v.ValidationErrors.Select(e => e.ErrorMessage)));
            else
                result.Add(exception.Message);
            return result;
        }

        public override void Handle(ExceptionHandlerContext context)
        {
            if (context.Exception is LicenseException)
            {
                Logger.Warn(context.Exception);
                context.Result = new ErrorResult(context.Request, new Messages.ResponseMessage<Exception> { Messages = { context.Exception.Message }, Data = context.Exception }, System.Net.HttpStatusCode.Forbidden);
            }
            else
            {
                if (!(context.Exception is HandledException)) Logger.Error(context.Exception, "An unhandled exception has occurred.");
                var messages = BuildErrorMessage(context.Exception);
                context.Result = new ErrorResult(context.Request, new Messages.ResponseMessage<Exception>(messages) { Data = context.Exception });
            }
        }
    }
}
