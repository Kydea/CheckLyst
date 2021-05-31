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

        public async Task<ReportResponse> GetReportData(IDefaultIdentity identity, ReportRequest request, CancellationToken cancellationToken)
        {
            var Report = await _reportsRepository.GetReportAsync(identity.CompanyId, request.Code);

            /*
            var dict = new Dictionary<string, object> { { "Property", "foo" } };
            dynamic eo = dict.Aggregate(new ExpandoObject() as IDictionary<string, Object>,
                                        (a, p) => { a.Add(p.Key, p.Value); return a; });
            string value = eo.Property;
            */

            var Parameters = request.Parameters.Select(p => {
                if (DataType.FromName("Date").TryParse(p.Value, out var Value))
                    return new Parameter(p.Code, Value);
                else
                    return new Parameter(p.Code, p.Value);
            });

            var Rows = await _reportViewerRepository.GetDataAsync(identity.CompanyId, await _settingService.TimeZoneAsync(identity.CompanyId), Report, Parameters, cancellationToken);

            //var Rows = await _reportViewerRepository.GetData(Report, request.Parameters.ToDictionary(p => p.Code, v => v.Value));
            return new ReportResponse
            {
                Description = Report.Description,
                Columns = Report.ReportColumns.Select(c => c.Label).ToList(),
                Rows = Rows
            };
        }
    }
}
 