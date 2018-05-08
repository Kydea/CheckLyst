using System.Threading.Tasks;
using System.Web.Http;
using System;
using System.Net.Http;
using System.Net;
using NLog;
using CheckListManager.ExternalSources.Logic;

namespace CheckListManager.ExternalSources.Api.Controllers
{
	public class BaseController: ApiController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [HttpPost]
        [Route("Test")]
        public IHttpActionResult Test(ExternalSourceRequest request)
        {
            try
            {
                using (var Source = SourceBase.Build(request.className, request.code, request.password, request.version, request.data))
                {
                    Source.Open();
                    Source.Test();
                    return Ok();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "External source error");
                throw;
            }
        }

        [HttpPost]
        [Route("GetMetaData")]
        public IHttpActionResult GetMetaData(ExternalSourceRequest request)
        {
            try
            {
                using (var Source = SourceBase.Build(request.className, request.code, request.password, request.version, request.data))
                    return Ok(new {
                        Code = request.code,
                        Source.Parameters,
                        Source.Columns
                    });
            }
            catch (Exception e)
            {
                Logger.Error(e, "External source error");
                throw;
            }
        }

        [HttpPost]
        [Route("GetData")]
        public IHttpActionResult GetData(ExternalSourceRequest request)
        {
            try
            {
                using (var Source = SourceBase.Build(request.className, request.code, request.password, request.version, request.data))
                {
                    Source.Open();
                    return Ok(new
                    {
                        Rows = Source.GetData(request.parameters, request.page)
                    });
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "External source error");
                throw;
            }
        }
    }
}
