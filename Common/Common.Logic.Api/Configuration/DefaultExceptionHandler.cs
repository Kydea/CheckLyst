using System.Web.Http.ExceptionHandling;
using NLog;

namespace CheckListManager.Common.Logic.Api.Configuration
{
	public class DefaultExceptionHandler : ExceptionHandler
	{
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

		public override void Handle(ExceptionHandlerContext context)
		{
			Logger.Error(context.Exception, "An unhandled exception has occurred.");
			context.Result = new ErrorResult(context);
		}
	}
}
