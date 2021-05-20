using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.DataEngine;
using CheckListManager.Common.Logic.DataSources;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CheckListManager.Reports.Logic.Data
{
    public interface IDataService : IService
    {
        Task<DataSource> GetDataSourceAsync(long Company, string Code);
        Task<PagingResponse<List<DataSource>>> GetDataSourcesAsync(long Company, PagingRequest<DataRequest> form);
        Task<string> GetDataSourceRowsAsync(long Company, Common.Logic.DataSources.DataRequest form);
        Task ExportAsync(long Company, ExportRequest form);
    }
}
