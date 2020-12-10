using CheckListManager.Common.Core.Network;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.DataEngine;
using CheckListManager.Common.Logic.DataSources;
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

            if (Record == null)
                throw new Exception($"Invalid external source");

            var Data = await CallExternalSource("GetMetaData", new Common.Logic.ExternalSources.DataRequest
            {
                code = $"{Company}.{Record.Code}",
                className = Record.Class,
                password = Record.Password,
                data = Record.Data,
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

            await CallExternalSource("Test", new Common.Logic.ExternalSources.DataRequest {
                code = $"{Company}.{Record.Code}",
                className = Record.Class,
                password = Record.Password,
                data = Record.Data,
                version = Record.ModifiedOn ?? Record.CreatedOn
            });
        }

        public async Task<string> GetDataSourceRowsAsync(long Company, Common.Logic.DataSources.DataRequest Request)
        {
            var Record = (await _dataSourcesRepository.GetDataSourcesAsync(
                Company,
                ds => new Common.Logic.ExternalSources.DataRequest
                {
                    code = ds.Code,
                    className = ds.Class,
                    password = ds.Password,
                    version = ds.ModifiedOn ?? ds.CreatedOn,
                    data = ds.Data
                },
                wf => wf.Company == Company && wf.Code == Request.code,
                p => p.Code,
                1,
                1
            )).Data.SingleOrDefault();

            Record.parameters = Request.parameters;
            Record.page = Request.page;
            Record.decimalSeparator = Request.decimalSeparator;
            Record.timeZone = Request.timeZone;

            return await CallExternalSource("GetData", Record);
        }

        private async Task<string> CallExternalSource(string Action, Common.Logic.ExternalSources.DataRequest Data)
        {
            try
            {
                var response = await _client.PostAsync("ExternalSources/" + Action, Data);
                var responseData = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                    throw new Exception(responseData);

                return responseData;
            }
            catch (Exception e)
            {
                throw new Exception($"External source error: {e.Message}", e);
            }

        }
    }
}