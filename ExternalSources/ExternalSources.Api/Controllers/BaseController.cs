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
                using (var Source = SourceBase.Build(request.DataSourceClass, request.DataSourceCode, request.Password, request.Version, request.DataSourceParameters))
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
                using (var Source = SourceBase.Build(request.DataSourceClass, request.DataSourceCode, request.Password, request.Version, request.DataSourceParameters))
                    return Ok(new {
                        Code = request.DataSourceCode,
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
    }
}
