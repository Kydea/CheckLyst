using System;
using System.Web.Http.ExceptionHandling;
using CheckListManager.Common.Logic.License;
using NLog;

namespace CheckListManager.Common.Logic.Api.Configuration
{
    public class DefaultExceptionHandler : ExceptionHandler
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public override void Handle(ExceptionHandlerContext context)
        {
            if (context.Exception is LicenseException)
            {
                Logger.Warn(context.Exception);
                context.Result = new ErrorResult(context.Request, new Messages.ResponseMessage<Exception> { Messages = { context.Exception.Message}, Data = context.Exception }, System.Net.HttpStatusCode.Forbidden);
            }
            else
            {
                string message = "An unhandled exception has occurred.";
                Logger.Error(context.Exception, message);
                context.Result = new ErrorResult(context.Request, new Messages.ResponseMessage<Exception> { Messages = { message }, Data = context.Exception });
            }
        }
    }
}
