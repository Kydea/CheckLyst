using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.DataSources.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CheckListManager.DataSources.Logic.Data
{
    public class DataService : IDataService
    {
        private readonly ICommonRepository _commonRepository;
        private readonly IDataSourcesRepository _dataSourcesRepository;

        public DataService(ICommonRepository commonRepository, IDataSourcesRepository dataSourcesRepository)
        {
            _commonRepository = commonRepository;
            _dataSourcesRepository = dataSourcesRepository;
        }
    }
}
