using CheckListManager.Common.Core.Network;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.DataEngine;
using CheckListManager.Common.Logic.Models;
using CheckListManager.Common.Logic.Services;
using CheckListManager.Reports.Data;
using CheckListManager.Reports.Logic.Detail;
using Newtonsoft.Json;
using NLog;
using SAP.Middleware.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CheckListManager.Reports.Logic.Viewer
{
    public class ReportViewerService : IViewerEditorService
    {
        private readonly IReportsRepository _reportsRepository;
        private readonly IReportViewerRepository _reportViewerRepository;
        private readonly ISettingService _settingService;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ReportViewerService(IReportsRepository reportsRepository, IReportViewerRepository reportViewerRepository, ISettingService settingService)
        {
            _reportsRepository = reportsRepository;
            _reportViewerRepository = reportViewerRepository;
            _settingService = settingService;
        }

        public async Task<DetailResponse> LoadParameters(IDefaultIdentity identity, string code)
        {
            var record = await _reportsRepository.GetReportAsync(identity.CompanyId, code);
            var result = new DetailResponse
            {
                Description = record.Description,
                Clauses = record.ReportClauses.Where(c => c.Visible).Select(c => new ReportClause
                {
                    Label = c.Label,
                    ParameterName = c.ParameterName,
                    Editable = c.Editable,
                    ParameterType = c.ParameterType,
                    ClauseType = c.ClauseType,
                    Clause = JsonConvert.DeserializeObject<ReportClause.ClauseRecord>(c.Clause),
                }).ToList()
            };
            return result;
        }

        private string ParseFilter(ReportClause.ClauseRecord Clause, string ParameterName) =>
            Clause.Sign == "c" ? $"q.\"{Clause.Column}\" like '%@{ParameterName}%'" :
            Clause.Sign == "!c" ? $"not q.\"{Clause.Column}\" like '%@{ParameterName}%'" :
            Clause.Sign == "0" ? $"q.\"{Clause.Column}\" is null" :
            Clause.Sign == "!0" ? $"not q.\"{Clause.Column}\" is null" :
            $"q.\"{Clause.Column}\" {Clause.Sign} @{ParameterName} ";

        private string ParseClause(ReportClauses ReportClause)
        {
            var Clause = JsonConvert.DeserializeObject<ReportClause.ClauseRecord>(ReportClause.Clause); //
            switch (ReportClause.ClauseType)
            {
                case "filter":
                    return ParseFilter(Clause, ReportClause.ParameterName);
                default:
                    throw new ArgumentException($"Invalid clause type {ReportClause.ClauseType}");
            }
        }

        private StringBuilder BuildQuery(long company, Common.Data.Reports report)
        {
            var StringBuilder = new StringBuilder();
            StringBuilder.AppendLine($"set role \"ReportUser{company}\";");
            StringBuilder.AppendLine($"with q as (");
            StringBuilder.AppendLine(report.Query);
            StringBuilder.AppendLine($")");
            StringBuilder.AppendLine($"select {string.Join(", ", report.ReportColumns.Select(c => $"{c.Column} \"{c.Label}\""))}");
            StringBuilder.AppendLine($"from q");
            StringBuilder.AppendLine($"where");
            StringBuilder.AppendLine($"    1 = 1");
            foreach (var clause in report.ReportClauses)
                StringBuilder.AppendLine($"    and {ParseClause(clause)}");
            StringBuilder.AppendLine($"limit 10"); //togliere
            return StringBuilder;
        }

        public async Task<ReportResponse> GetReportData(IDefaultIdentity identity, ReportRequest request, CancellationToken cancellationToken)
        {
            var Report = await _reportsRepository.GetReportAsync(identity.CompanyId, request.Code);

            var Parameters = request.Parameters.Select(p => {
                if (DataType.FromName("Date").TryParse(p.Value, out var Value))
                    return new Parameter(p.Code, Value);
                else
                    return new Parameter(p.Code, p.Value);
            });

            var query = BuildQuery(identity.CompanyId, Report).ToString();

            var rows = await _reportViewerRepository.GetDataAsync(identity.CompanyId, await _settingService.TimeZoneAsync(identity.CompanyId), query, Parameters, cancellationToken);
            return new ReportResponse
            {
                Description = Report.Description,
                Columns = Report.ReportColumns.Select(c => c.Label).ToList(),
                Rows = rows
            };
        }
    }
}
 