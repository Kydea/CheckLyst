using CheckListManager.Common.Core.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Filters;

namespace CheckListManager.Common.Logic.Api.Filters
{
    public class CompressionAttribute : ActionFilterAttribute
    {
        private bool IsCompressionSupported()
        {
            string AcceptEncoding = HttpContext.Current.Request.Headers["Accept-Encoding"];
            return ((!string.IsNullOrEmpty(AcceptEncoding) && (AcceptEncoding.Contains("gzip") || AcceptEncoding.Contains("deflate"))));
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionContext)
        {
            bool isCompressionSupported = IsCompressionSupported();
            string acceptEncoding = HttpContext.Current.Request.Headers["Accept-Encoding"];

            if (isCompressionSupported)
            {
                var content = actionContext.Response.Content;
                var byteArray = content?.ReadAsByteArrayAsync().Result;
                MemoryStream memoryStream = new MemoryStream(byteArray);
                if (acceptEncoding.Contains("gzip"))
                {
                    actionContext.Response.Content = new ByteArrayContent(CompressionHelper.Compress(memoryStream.ToArray(), true));
                    actionContext.Response.Content.Headers.Remove("Content-Type");
                    actionContext.Response.Content.Headers.Add("Content-encoding", "gzip");
                    actionContext.Response.Content.Headers.Add("Content-Type", "application/json");
                }
                else
                {
                    actionContext.Response.Content = new ByteArrayContent(CompressionHelper.Compress(memoryStream.ToArray(), false));
                    actionContext.Response.Content.Headers.Remove("Content-Type");
                    actionContext.Response.Content.Headers.Add("Content-encoding", "deflate");
                    actionContext.Response.Content.Headers.Add("Content-Type", "application/json");
                }
            }

            base.OnActionExecuted(actionContext);
        }
    }
}
