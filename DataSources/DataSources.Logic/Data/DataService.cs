using CheckListManager.Common.Core.Network;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.DataEngine;
using CheckListManager.DataSources.Data;
using NLog;
using SAP.Middleware.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CheckListManager.DataSources.Logic.Data
{
    public class DataService : IDataService
    {
        private readonly IDataSourcesRepository _dataSourcesRepository;
        private readonly IServiceClient _client;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public DataService(IDataSourcesRepository dataSourcesRepository, IServiceClient client)
        {
            _dataSourcesRepository = dataSourcesRepository;
            _client = client;
        }

        public async Task<DataSource> GetDataSourceAsync(long Company, DataRequest Form)
            =>
                (await _dataSourcesRepository.GetDataSourcesAsync(
                    Company,
                    ds => new DataSource
                    {
                        Code = ds.Code,
                        Class = ds.Class,
                        Data = ds.Data
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
                    ds => new DataSource
                    {
                        Code = ds.Code, 
                        Class = ds.Class,
                        Data = ds.Data
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

            var DataSource = new DataSource {
                Data = Record.Data,
                Password = Record.Password,
                Class = Record.Class
            };

            await CallExternalSource("Test", new { });
        }

        private async Task<string> CallExternalSource<T>(string Action, T Data)
        {
            try
            {
                var response = await _client.PostAsync("ExternalSources/" + Action, Data);
                var responseData = await response.Content.ReadAsStringAsync();
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    throw new Exception(responseData);
                return responseData;
            }
            catch (Exception e)
            {
                throw new Exception("External source error", e);
            }

        }
    }
}