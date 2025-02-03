using Newtonsoft.Json;
using SAP.Middleware.Connector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.ExternalSources.Logic.SAP
{
    public class Source : SourceBase
    {
        private readonly object Lock = new object();

        private static SAPConfiguration SAPConfiguration { get; set; }

        private RfcDestination Destination { get; set; }
        private SAPParameters SAPParameters { get; set; }

        public override void Parse(string Data)
        {
            SAPParameters = JsonConvert.DeserializeObject<SAPParameters>(Data);
            BuildParameters();
            BuildColumns();
        }

        public override void Open()
        {
            lock (Lock)
            {
                if (SAPConfiguration == null)
                    RfcDestinationManager.RegisterDestinationConfiguration(SAPConfiguration = new SAPConfiguration());
                SAPConfiguration.Check(Code, SAPParameters, Version, Password);
            }

            Destination = RfcDestinationManager.GetDestination(Code);
        }

        public override void Dispose()
        {
        }

        private void BuildParameters()
        {
            switch (SAPParameters.FunctionName)
            {
                case "PO Header":
                    Parameters = new List<Parameter> { new Parameter { Label = "PO Number", Name = "PO", ParameterType = "Text" } };
                    break;
                default:
                    throw new NotSupportedException($"Source name {SAPParameters.FunctionName}");
            }
        }

        private void BuildColumns()
        {
            switch (SAPParameters.FunctionName)
            {
                case "PO Header":
                    Columns = new List<string>
                    {
                        "PO",
                        "Type",
                        "Line",
                        "Operation",
                        "Operation Description",
                        "Material Number",
                        "Material Description",
                        "Batch",
                        "Scheduled Quantity",
                        "Effective Quantity"
                    };
                    break;
                default:
                    throw new NotSupportedException($"Source name {SAPParameters.FunctionName}");
            }
        }

        public override void PrepareMetaData()
        {
            OperationTime = 0;
        }

        public override Task<IEnumerable<List<string>>> GetDataAsync(Dictionary<string, string> Parameters, int Page, string DecimalSeparator, string TimeZone)
        {
            var stopWatch = Stopwatch.StartNew();
            switch (SAPParameters.FunctionName)
            {
                case "PO Header":
                    var Result = FunctionPOHeader(Parameters, Page, DecimalSeparator);
                    stopWatch.Stop();
                    OperationTime = stopWatch.Elapsed.TotalMilliseconds;
                    return Task.FromResult(Result.AsEnumerable());
                default:
                    throw new NotSupportedException($"Source name {SAPParameters.FunctionName}");
            }
        }

        #region Functions

        public IEnumerable<List<string>> FunctionPOHeader(Dictionary<string, string> Parameters, int Page, string DecimalSeparator)
        {
            if (string.IsNullOrWhiteSpace(Parameters["PO"]))
                yield break;

            var PO = Parameters["PO"]?.Trim().PadLeft(12, '0');

            var Repository = Destination.Repository;
            IRfcFunction function = Repository.CreateFunction("BAPI_PROCORD_GET_DETAIL");

            IRfcStructure Structure = function.GetStructure("ORDER_OBJECTS");
            Structure.SetValue("HEADER", "X");
            Structure.SetValue("POSITIONS", "X");
            Structure.SetValue("SEQUENCES", "X");
            Structure.SetValue("COMPONENTS", "X");
            Structure.SetValue("PHASES", "X");
            Structure.SetValue("PROD_REL_TOOLS", "X");
            Structure.SetValue("TRIGGER_POINTS", "X");

            function.SetValue("NUMBER", PO);

            IRfcStructure Return = function.GetStructure("RETURN");
            var Message = Return.GetString("MESSAGE");
            var Header = function.GetTable("HEADER");
            var Positions = function.GetTable("POSITION");
            var Sequences = function.GetTable("SEQUENCE");
            var Components = function.GetTable("COMPONENT");
            var Phase = function.GetTable("PHASE");
            var ProdRelTools = function.GetTable("PROD_REL_TOOL");
            var TriggerPoints = function.GetTable("TRIGGER_POINT");
            function.Invoke(Destination);

            var Row = 0;
            string Line = null;
            var currentPage = Page < 0 ? 0 : Page;
            for (var PhaseIndex = 0; PhaseIndex < Phase.RowCount && Row < GetPageSize(Page) * (currentPage + 1); ++PhaseIndex)
            {
                Phase.CurrentIndex = PhaseIndex;
                if (Line != (Line = Phase.GetString("RESOURCE"))) //Takes the first row for each resource
                {
                    if (Row >= GetPageSize(Page) * currentPage)
                        yield return new List<string> {
                            Header.GetString("ORDER_NUMBER")?.TrimStart('0'),
                            Header.GetString("PROD_SCHED_PROFILE"),
                            Line,
                            Phase.GetString("OPERATION_NUMBER"),
                            Phase.GetString("DESCRIPTION"),
                            Header.GetString("MATERIAL")?.TrimStart('0'),
                            Header.GetString("MATERIAL_TEXT"),
                            Header.GetString("BATCH"),
                            Format(Positions.GetDecimal("QUANTITY"), DecimalSeparator),
                            Format(Positions.GetDecimal("DELIVERED_QUANTITY"), DecimalSeparator)
                        };
                    ++Row;
                }
            }
        }

        #endregion
    }
}
