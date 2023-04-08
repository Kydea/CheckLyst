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

        public async Task<DetailResponse> LoadParameters(IDefaultIdentity identity, long id)
        {
            var record = await _reportsRepository.GetReportAsync(identity.CompanyId, id, identity.HasPermission("Reports", "Edit") ? null : identity.Code);
            var result = new DetailResponse
            {
                Code = record.Code,
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

        private string ParseFilter(ReportClause.ClauseRecord Clause, string ParameterName, object Value) =>
            Clause.Sign == "0" ? Value as bool? == true ? $"q.\"{Clause.Column}\" is null" : null :
            Clause.Sign == "!0" ? Value as bool? == true ? $"not q.\"{Clause.Column}\" is null" : null :
            Clause.CaseInsensitive ?
                Clause.Sign == "c" ? $"upper(q.\"{Clause.Column}\") like '%' || upper(@{ParameterName}) || '%'" :
                Clause.Sign == "!c" ? $"not upper(q.\"{Clause.Column}\") like '%' || upper(@{ParameterName}) || '%'" :
                Clause.Sign == "s" ? $"upper(q.\"{Clause.Column}\") like upper(@{ParameterName}) || '%'" :
                Clause.Sign == "!s" ? $"not upper(q.\"{Clause.Column}\") like upper(@{ParameterName}) || '%'" :
                $"upper(q.\"{Clause.Column}\") {Clause.Sign} upper(@{ParameterName}) " :

                Clause.Sign == "c" ? $"q.\"{Clause.Column}\" like '%' || @{ParameterName} || '%'" :
                Clause.Sign == "!c" ? $"not q.\"{Clause.Column}\" like '%' || @{ParameterName} || '%'" :
                Clause.Sign == "s" ? $"q.\"{Clause.Column}\" like @{ParameterName} || '%'" :
                Clause.Sign == "!s" ? $"not q.\"{Clause.Column}\" like @{ParameterName} || '%'" :
                $"q.\"{Clause.Column}\" {Clause.Sign} @{ParameterName} ";

        private string ParseClause(ReportClauses ReportClause, ReportClause.ClauseRecord Clause, object Value)
        {
            switch (ReportClause.ClauseType)
            {
                case "filter":
                    return ParseFilter(Clause, ReportClause.ParameterName, Value);
                case "custom":
                    return $"({Clause.Custom})";
                case "parameter":
                    return null;
                default:
                    throw new ArgumentException($"Invalid clause type {ReportClause.ClauseType}");
            }
        }

        private StringBuilder BuildQuery(long company, Common.Data.Reports report, List<Parameter> parameters, Dictionary<string, string> requestValues)
        {
            var StringBuilder = new StringBuilder();
            StringBuilder.AppendLine($"set role \"ReportUser{company}\";");
            StringBuilder.AppendLine($"with q as (");
            StringBuilder.AppendLine(report.ReportSource == null ? report.Query : report.ReportSources.Query);
            StringBuilder.AppendLine($")");
            StringBuilder.AppendLine($"select {string.Join(", ", report.ReportColumns.OrderBy(rc => rc.ID).Select(c => $"{c.Column} \"{c.Label}\""))}");
            StringBuilder.AppendLine($"from q");
            StringBuilder.AppendLine($"where");
            StringBuilder.AppendLine($"    1 = 1");
            foreach (var ReportClause in report.ReportClauses)
            {
                var Clause = JsonConvert.DeserializeObject<ReportClause.ClauseRecord>(ReportClause.Clause);
                var ParameterValue = ReportClause.Editable && requestValues != null && requestValues.TryGetValue(ReportClause.ParameterName, out var RequestValue) ? RequestValue : Clause.Value;
                if (!string.IsNullOrEmpty(ParameterValue))
                {
                    if (!DataType.FromName(ReportClause.ParameterType).TryParse(ParameterValue, out var TypedValue))
                        throw new ReportViewerException($"Invalid value {ParameterValue} for type {ReportClause.ParameterType}");
                    var clause = ParseClause(ReportClause, Clause, TypedValue);
                    if (clause != null)
                        StringBuilder.AppendLine($"    and {clause}");
                    parameters.Add(new Parameter(ReportClause.ParameterName, TypedValue));
                }
            }
            var OrderColumns = report.ReportColumns.Where(c => c.OrderColumn != 0);
            if (OrderColumns.Any())
                StringBuilder.AppendLine($"order by {string.Join(", ", OrderColumns.Select(c => c.Column + (c.OrderColumn == -1 ? " desc" : string.Empty)))}");
            return StringBuilder;
        }

        public async Task<ReportResponse> GetReportData(IDefaultIdentity identity, ReportRequest request, CancellationToken cancellationToken)
        {
            var report = await _reportsRepository.GetReportAsync(identity.CompanyId, request.ID, identity.HasPermission("Reports", "Edit") ? null : identity.Code);
            var parameters = new List<Parameter>(report.ReportClauses.Count);
            var query = BuildQuery(identity.CompanyId, report, parameters, request.Parameters?.ToDictionary(k => k.Code, v => v.Value)).ToString();
            var rows = await _reportViewerRepository.GetDataAsync(identity.CompanyId, await _settingService.TimeZoneAsync(identity.CompanyId), await _settingService.DecimalSeparatorAsync(identity.CompanyId), query, report.ReportColumns.ToList(), parameters, cancellationToken);
            return new ReportResponse
            {
                ID = report.ID,
                Code = report.Code,
                Description = report.Description,
                Online = report.Online,
                Columns = report.ReportColumns.Where(c => c.Visible).Select(c => c.Label).ToList(),
                Rows = rows
            };
        }

        public async Task<string> GetReportQuery(IDefaultIdentity identity, ReportRequest request, CancellationToken cancellationToken)
        {
            var report = await _reportsRepository.GetReportAsync(identity.CompanyId, request.ID, null);
            var parameters = new List<Parameter>(report.ReportClauses.Count);
            var query = BuildQuery(identity.CompanyId, report, parameters, request.Parameters?.ToDictionary(k => k.Code, v => v.Value)).ToString();
            return query;
        }
    }
}
