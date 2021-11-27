using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Core.Web.Security;
using System.Data;
using System.Threading;
using CheckListManager.Reports.Data.Models;

namespace CheckListManager.Reports.Data
{
    public interface IReportViewerRepository : IRepository
    {
        Task<DataTable> GetDataTableAsync(long company, string command, Dictionary<string, string> parameters, CancellationToken cancellationToken = default(CancellationToken));
        Task<List<ReportRow>> GetDataAsync(long company, string timeZone, string decimalSeparator, string query, List<ReportColumns> columns, IEnumerable<Parameter> parameters, CancellationToken cancellationToken);
    }
}