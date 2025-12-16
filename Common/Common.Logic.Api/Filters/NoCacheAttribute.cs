using System;
using System.Net.Http.Headers;
using System.Web.Http.Filters;

namespace CheckListManager.Common.Logic.Api.Filters
{
    public sealed class NoCacheAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            var resp = actionExecutedContext.Response;
            if (resp == null) return;

            resp.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true,
                NoStore = true,
                MustRevalidate = true
            };
            resp.Headers.Pragma.ParseAdd("no-cache");
        }
    }
}
