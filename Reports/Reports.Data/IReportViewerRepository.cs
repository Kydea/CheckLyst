using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Core.Web.Security;
using System.Data;

namespace CheckListManager.Reports.Data
{
    public interface IReportViewerRepository : IRepository
    {
        Task<DataTable> GetDataTable(long company, string command);
        Task<List<List<string>>> GetData(long company, Common.Data.Reports report);
    }
}