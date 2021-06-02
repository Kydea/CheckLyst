using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Core.Web.Security;
using System.Data;
using System.Threading;

namespace CheckListManager.Reports.Data
{
    public interface IReportViewerRepository : IRepository
    {
        Task<DataTable> GetDataTableAsync(long company, string command, CancellationToken cancellationToken = default(CancellationToken));
        Task<List<List<string>>> GetDataAsync(long company, string timeZone, string query, IEnumerable<Parameter> parameters, CancellationToken cancellationToken);
    }
}