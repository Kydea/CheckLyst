using System.Threading.Tasks;
using System.Web.Http;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Web.Http.ExceptionHandling;
using System;
using NLog;
using CheckListManager.ExternalSources.Logic;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace CheckListManager.ExternalSources.Api.Controllers
{
	public class BaseController: ApiController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static long OperationNumber = 0;

        private void BuildExceptionMessage(StringBuilder Message, Exception Exception)
        {
            var message = new StringBuilder();
            if (Exception is AggregateException AggregateException)
            {
                AggregateException.InnerExceptions.ToList().ForEach(e =>
                {
                    Message.Append(" - ");
                    BuildExceptionMessage(Message, e);
                });
            }
            else
                Message.Append(Exception.Message);
        }

        private IHttpActionResult Error(Exception Exception)
        {
            Logger.Error(Exception, "External source error");

            var Message = new StringBuilder();
            BuildExceptionMessage(Message, Exception);
            return Content(HttpStatusCode.InternalServerError, Message.ToString());
        }

        private long GetOperationNumber() => OperationNumber = OperationNumber == long.MaxValue ? 0 : OperationNumber + 1;

        [HttpPost]
        [Route("GetMetaData")]
        public IHttpActionResult GetMetaData(ExternalSourceRequest request)
        {
            try
            {
                var OperationNumber = GetOperationNumber();
                Logger.Info($"ExternalSource/GetMetaData {OperationNumber} parameters:\n {JsonConvert.SerializeObject(request)}");
                using (var Source = SourceBase.Build(request.className, request.code, request.password, request.version, request.data))
                {
                    Source.PrepareMetaData(out var OperationTime);
                    Logger.Info($"ExternalSource/GetMetaData {OperationNumber} time (ms): {OperationTime}");
                    return Ok(new
                    {
                        Code = request.code,
                        Source.Parameters,
                        Source.Columns
                    });
                }
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        [HttpPost]
        [Route("GetData")]
        public IHttpActionResult GetData(ExternalSourceRequest request)
        {
            try
            {
                var OperationNumber = GetOperationNumber();
                Logger.Info($"ExternalSource/GetData {OperationNumber} parameters:\n{JsonConvert.SerializeObject(request)}");
                using (var Source = SourceBase.Build(request.className, request.code, request.password, request.version, request.data))
                {
                    Source.Open();
                    var Result = Source.GetData(request.parameters, request.page, request.decimalSeparator, request.timeZone, out var OperationTime);
                    Logger.Info($"ExternalSource/GetData {OperationNumber} time (ms): {OperationTime}");
                    return Ok(new
                    {
                        Rows = request.page > 4 ? //limited to maximum 5 pages
                            null :
                            Result
                    });
                }
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        [HttpPost]
        [Route("Export")]
        public IHttpActionResult Export(ExternalSourceRequest request)
        {
            try
            {
                double TotalOperationTime = 0;
                var OperationNumber = GetOperationNumber();
                Logger.Info($"ExternalSource/Export {OperationNumber} parameters:\n{JsonConvert.SerializeObject(request)}");
                using (var Source = SourceBase.Build(request.className, request.code, request.password, request.version, request.data))
                {
                    Source.Open();

                    foreach (var Record in request.records)
                    {
                        var Parameters = new Dictionary<string, string>();
                        for (var Index = 0; Index < request.names.Count; ++Index)
                            Parameters.Add(request.names[Index], Record[Index]);
                        Source.GetData(Parameters, request.page, request.decimalSeparator, request.timeZone, out var OperationTime);
                        TotalOperationTime += OperationTime;
                    }
                    Logger.Info($"ExternalSource/Export {OperationNumber} time (ms): {TotalOperationTime}");

                    return Ok();
                }
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

    }
}
