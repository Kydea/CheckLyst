using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Http.Validation;
using CheckListManager.Common.Logic.Api.Json;

namespace CheckListManager.Common.Logic.Api.Configuration
{
    public class CustomFormatterParameterBinding : FormatterParameterBinding
    {
        public CustomFormatterParameterBinding(HttpParameterDescriptor descriptor, IEnumerable<MediaTypeFormatter> formatters, IBodyModelValidator bodyModelValidator) : base(descriptor, formatters, bodyModelValidator)
        {
        }

        public override Task<object> ReadContentAsync(HttpRequestMessage request, Type type, IEnumerable<MediaTypeFormatter> formatters, IFormatterLogger formatterLogger)
        {
            return base.ReadContentAsync(request, type, GetFormatters(formatters, request), formatterLogger);
        }

        public override Task<object> ReadContentAsync(HttpRequestMessage request, Type type, IEnumerable<MediaTypeFormatter> formatters, IFormatterLogger formatterLogger,
            CancellationToken cancellationToken)
        {
            return base.ReadContentAsync(request, type, GetFormatters(formatters, request), formatterLogger, cancellationToken);
        }

        private IEnumerable<MediaTypeFormatter> GetFormatters(IEnumerable<MediaTypeFormatter> formatters, HttpRequestMessage request)
        {
            foreach (var formatter in formatters)
                if (formatter is JsonMediaTypeFormatter jsonFormatter)
                    yield return new CustomJsonFormatter(jsonFormatter, request);
                else
                    yield return formatter;
        }
    }
}