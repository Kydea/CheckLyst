using System.Linq;
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
            return !string.IsNullOrEmpty(AcceptEncoding) && (AcceptEncoding.Contains("gzip") || AcceptEncoding.Contains("deflate"));
        }

        public override async Task OnActionExecutedAsync(HttpActionExecutedContext actionContext, CancellationToken cancellationToken)
        {
            var isCompressionSupported = IsCompressionSupported();
            var content = actionContext?.Response?.Content;

            if (isCompressionSupported && content != null)
            {
                var acceptEncoding = HttpContext.Current.Request.Headers["Accept-Encoding"];
                var contentType = actionContext.Response.Content.Headers.ContentType;
                var contentDisposition = actionContext.Response.Content.Headers.ContentDisposition;
                var byteArray = await content.ReadAsByteArrayAsync();
                if (acceptEncoding.Contains("gzip"))
                {
                    actionContext.Response.Content = new ByteArrayContent(CompressionHelper.Compress(byteArray, true));
                    actionContext.Response.Content.Headers.Add("Content-encoding", "gzip");
                }
                else
                {
                    actionContext.Response.Content = new ByteArrayContent(CompressionHelper.Compress(byteArray, false));
                    actionContext.Response.Content.Headers.Add("Content-encoding", "deflate");
                }
                if (contentType != null) actionContext.Response.Content.Headers.ContentType = contentType;
                if (contentDisposition != null) actionContext.Response.Content.Headers.ContentDisposition = contentDisposition;
            }
            
            await base.OnActionExecutedAsync(actionContext, cancellationToken);
        }
    }
}
