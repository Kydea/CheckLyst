using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Globalization;
using System.Diagnostics;

namespace CheckListManager.ExternalSources.Logic.REST
{
    public class Source : SourceBase
    {
        private RESTParameters RESTParameters { get; set; }

        public override void Dispose()
        {
        }

        public override IEnumerable<List<string>> GetData(Dictionary<string, string> Parameters, int Page, string DecimalSeparator, string TimeZone, out double OperationTime)
        {
            var stopWatch = Stopwatch.StartNew();
            OperationTime = stopWatch.Elapsed.TotalMilliseconds;

            return null;
        }

        public override void Open()
        {
        }

        public override void Parse(string Data)
        {
            RESTParameters = JsonConvert.DeserializeObject<RESTParameters>(Data);
            Parameters = RESTParameters.QueryParameters ?? new List<Parameter>(0);
        }

        public override void PrepareMetaData(out double OperationTime)
        {
            OperationTime = 0;
        }
    }
}
