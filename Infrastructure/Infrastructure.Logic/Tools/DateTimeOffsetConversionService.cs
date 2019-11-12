using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.DataEngine;
using CheckListManager.Common.Logic.Queue;
using CheckListManager.Common.Logic.Services;
using CheckListManager.Common.Logic.Trail;
using CheckListManager.Infrastructure.Data;
using Newtonsoft.Json;

namespace CheckListManager.Infrastructure.Logic.Tools
{
    public static class DateTimeOffsetConversionService
    {
        public static DateTimeOffsetConversionResponse Process(DateTimeOffsetConversionRequest request)
        {
            var response = new DateTimeOffsetConversionResponse {
                DateTimeOffsets = request.DateTimeOffsets?.Distinct().ToDictionary(k => k, v => v.Format(request.TimeZone, request.OutputFormat)),
                Strings = request.Strings?.Distinct().ToDictionary(k => k, v => new DateTimeOffsetConversionRow(v, request.InputFormat, request.TimeZone))
            };

            return response;
        }
    }
}
