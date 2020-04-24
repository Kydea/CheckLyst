using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Filters;
using CheckListManager.Common.Core.Helpers;

namespace CheckListManager.Common.Logic.Api.Filters
{
    public class CompressionAttribute : ActionFilterAttribute
    {
        private bool IsCompressionSupported()
        {
            var AcceptEncoding = HttpContext.Current.Request.Headers["Accept-Encoding"];
            return (!string.IsNullOrEmpty(AcceptEncoding) && (AcceptEncoding.Contains("gzip") || AcceptEncoding.Contains("deflate")));
        }

        public override async Task OnActionExecutedAsync(HttpActionExecutedContext actionContext, CancellationToken cancellationToken)
        {
            var isCompressionSupported = IsCompressionSupported();
            var acceptEncoding = HttpContext.Current.Request.Headers["Accept-Encoding"];

            if (isCompressionSupported)
            {
                var content = actionContext.Response.Content;
                if (content != null)
                {
                    var byteArray = await content.ReadAsByteArrayAsync();
                    if (acceptEncoding.Contains("gzip"))
                    {
                        actionContext.Response.Content = new ByteArrayContent(CompressionHelper.Compress(byteArray, true));
                        actionContext.Response.Content.Headers.Remove("Content-Type");
                        actionContext.Response.Content.Headers.Add("Content-encoding", "gzip");
                        actionContext.Response.Content.Headers.Add("Content-Type", "application/json");
                    }
                    else
                    {
                        actionContext.Response.Content = new ByteArrayContent(CompressionHelper.Compress(byteArray, false));
                        actionContext.Response.Content.Headers.Remove("Content-Type");
                        actionContext.Response.Content.Headers.Add("Content-encoding", "deflate");
                        actionContext.Response.Content.Headers.Add("Content-Type", "application/json");
                    }
                }
            }
            
            await base.OnActionExecutedAsync(actionContext, cancellationToken);
        }
    }
}
