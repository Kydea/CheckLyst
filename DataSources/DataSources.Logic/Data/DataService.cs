using CheckListManager.Common.Core.Network;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.DataEngine;
using CheckListManager.DataSources.Data;
using Newtonsoft.Json;
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

        public async Task<DataSource> GetDataSourceAsync(long Company, string Code)
        {
            var Record = (await _dataSourcesRepository.GetDataSourcesAsync(
                Company,
                ds => ds,
                wf => wf.Company == Company && wf.Code == Code,
                p => p.Code,
                1,
                1
            )).Data.SingleOrDefault();

            var Data = await CallExternalSource("GetMetaData", new
            {
                dataSourceCode = $"{Company}.{Record.Code}",
                dataSourceClass = Record.Class,
                password = Record.Password,
                dataSourceParameters = Record.Data,
                version = Record.ModifiedOn ?? Record.CreatedOn
            });

            return JsonConvert.DeserializeObject<DataSource>(Data);
        }

        public async Task<PagingResponse<List<DataSource>>> GetDataSourcesAsync(long Company, PagingRequest<DataRequest> Form)
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

            await CallExternalSource("Test", new {
                dataSourceCode = $"{Company}.{Record.Code}",
                dataSourceClass = Record.Class,
                password = Record.Password,
                dataSourceParameters = Record.Data,
                version = Record.ModifiedOn ?? Record.CreatedOn
            });
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