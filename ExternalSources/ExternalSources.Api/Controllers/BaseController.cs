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

namespace CheckListManager.ExternalSources.Api.Controllers
{
	public class BaseController: ApiController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private IHttpActionResult Error(Exception Exception)
        {
            Logger.Error(Exception, "External source error");
            return Content(HttpStatusCode.InternalServerError, Exception.Message);
        }

        [HttpPost]
        [Route("GetMetaData")]
        public IHttpActionResult GetMetaData(ExternalSourceRequest request)
        {
            try
            {
                Logger.Info("ExternalSource/GetMetaData parameters:\n" + JsonConvert.SerializeObject(request));
                using (var Source = SourceBase.Build(request.className, request.code, request.password, request.version, request.data))
                {
                    Source.PrepareMetaData();
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
                Logger.Info("ExternalSource/GetData parameters:\n" + JsonConvert.SerializeObject(request));
                using (var Source = SourceBase.Build(request.className, request.code, request.password, request.version, request.data))
                {
                    Source.Open();
                    return Ok(new
                    {
                        Rows = request.page > 4 ? //limited to maximum 5 pages
                            null : 
                            Source.GetData(request.parameters, request.page, request.decimalSeparator, request.timeZone)
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
                Logger.Info("ExternalSource/Export parameters:\n" + JsonConvert.SerializeObject(request));
                using (var Source = SourceBase.Build(request.className, request.code, request.password, request.version, request.data))
                {
                    Source.Open();

                    foreach (var Record in request.records)
                    {
                        var Parameters = new Dictionary<string, string>();
                        for (var Index = 0; Index < request.names.Count; ++Index)
                            Parameters.Add(request.names[Index], Record[Index]);
                        Source.GetData(Parameters, request.page, request.decimalSeparator, request.timeZone);
                    }

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
