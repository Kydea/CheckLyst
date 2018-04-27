using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.DataEngine;
using CheckListManager.DataSources.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CheckListManager.DataSources.Logic.Data
{
    public class DataService : IDataService
    {
        private readonly IDataSourcesRepository _dataSourcesRepository;

        public DataService(IDataSourcesRepository dataSourcesRepository)
        {
            _dataSourcesRepository = dataSourcesRepository;
        }

        public async Task<DataSource> GetDataSourceAsync(long Company, DataRequest Form)
            =>
                (await _dataSourcesRepository.GetDataSourcesAsync(
                    Company,
                    ds => new DataSource(ds.Class)
                    {
                        Code = ds.Code
                    },
                    wf => wf.Company == Company && wf.Code == Form.Code,
                    p => p.Code,
                    0,
                    1
                )).Data.SingleOrDefault();

        public async Task<PagingResponse<List<DataSource>>> GetDataSourcesAsync(long Company, PagingRequest<Request> Form)
            =>
                await _dataSourcesRepository.GetDataSourcesAsync(
                    Company,
                    ds => new DataSource(ds.Class)
                    {
                        Code = ds.Code,                        
                    },
                    wf => wf.Company == Company,
                    p => p.Code,
                    Form.CurrentPage,
                    Form.ItemsPerPage
                );

        public async Task TestAsync(long Company, string Code)
        {
            var Record = (await _dataSourcesRepository.GetDataSourcesAsync(
                Company,
                ds => ds,
                wf => wf.Company == Company && wf.Code == Code,
                p => p.Code,
                1,
                1
            )).Data.SingleOrDefault();

            var DataSource = new DataSource(Record.Class);
            DataSource.Data = Record.Data;
        }
    }
}