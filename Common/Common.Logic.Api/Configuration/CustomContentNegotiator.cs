using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using CheckListManager.Common.Logic.Api.Json;

namespace CheckListManager.Common.Logic.Api.Configuration
{
    public class CustomContentNegotiator : DefaultContentNegotiator
    {
        public override ContentNegotiationResult Negotiate(Type type, HttpRequestMessage request, IEnumerable<MediaTypeFormatter> formatters)
        {
            var result = base.Negotiate(type, request, formatters);

            if (result.Formatter is JsonMediaTypeFormatter jsonFormatter)
                result.Formatter = new CustomJsonFormatter(jsonFormatter, request);

            return result;
        }
    }
}