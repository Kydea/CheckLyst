using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.DataEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CheckListManager.DataSources.Logic.Data
{
    public interface IDataService : IService
    {
        Task<DataSource> GetDataSourceAsync(long Company, DataRequest form);
        Task<PagingResponse<List<DataSource>>> GetDataSourcesAsync(long Company, PagingRequest<Request> form);
        Task TestAsync(long Company, string Code);
    }
}
