using System;
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
            var response = actionContext?.Response;
            var content = response?.Content;
            var mediaType = content?.Headers.ContentType?.MediaType;

            var isCompressed = content != null && content.Headers.ContentEncoding != null && content.Headers.ContentEncoding.Any();
            var isJson = content != null && (string.Equals(mediaType, "application/json", StringComparison.OrdinalIgnoreCase) || (mediaType != null && mediaType.EndsWith("+json", StringComparison.OrdinalIgnoreCase)));
            var isCsv = content != null && string.Equals(mediaType, "text/csv", StringComparison.OrdinalIgnoreCase);

            if (content != null && IsCompressionSupported() && !isCompressed && (isJson || isCsv))
            {
                var acceptEncoding = HttpContext.Current.Request.Headers["Accept-Encoding"] ?? "gzip";
                response.Content = new CompressedContent(content, acceptEncoding.Contains("gzip"));
            }
            
            await base.OnActionExecutedAsync(actionContext, cancellationToken);
        }
    }
}
