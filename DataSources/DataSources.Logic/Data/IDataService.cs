using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.DataEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CheckListManager.DataSources.Logic.Data
{
    public interface IDataService : IService
    {
        Task<DataSource> GetDataSourceAsync(long Company, string Code);
        Task<PagingResponse<List<DataSource>>> GetDataSourcesAsync(long Company, PagingRequest<DataRequest> form);
        Task TestAsync(long Company, string Code);
        Task<string> GetDataSourceRowsAsync(long Company, PagingRequest<DataRequest> form);
    }
}
