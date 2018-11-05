using Newtonsoft.Json;
using SAP.Middleware.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.ExternalSources.Logic.SQL
{
    public class Source : SourceBase
    {
        private SQLServerParameters SQLServerParameters { get; set; }

        public override void Parse(string Data)
        {
            SQLServerParameters = JsonConvert.DeserializeObject<SQLServerParameters>(Data);
            BuildParameters();
            BuildColumns();
        }

        public override void Open()
        {
        }

        public override void Dispose()
        {
        }

        private void BuildParameters()
        {
            Parameters = new List<Parameter> { new Parameter { Label = "PO Number", Name = "PO", ParameterType = "Text" } };
        }

        private void BuildColumns()
        {
            Columns = new List<string>
                {
                    "C1",
                    "C2",
                    "C3"
                };
        }   

        public override void Test()
        {
        }

        public override IEnumerable<List<string>> GetData(Dictionary<string, string> Parameters, int Page, string DecimalSeparator)
        {
            return null;
        }
    }
}
