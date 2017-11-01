using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.DataEngine;
using CheckListManager.DataSources.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.DataSources.Logic.List
{
    public class ListService : IListService
    {
        private readonly ICommonRepository _commonRepository;
        private readonly IDataSourcesRepository _dataSourcesRepository;

        public ListService(ICommonRepository commonRepository, IDataSourcesRepository dataSourcesRepository)
        {
            _commonRepository = commonRepository;
            _dataSourcesRepository = dataSourcesRepository;
        }
        
        public async Task<PagingResponse<List<ListRow>>> GetDataAsync(long Company, PagingRequest<ListRequest> Form)
            =>
                await _dataSourcesRepository.GetDataSourcesAsync(
                    Company,
                    ds => new ListRow
                    {
                        Code = ds.Code,
                        Description = ds.Description,
                        CreatedBy = ds.CreatedBy,
                        CreatedOn = ds.CreatedOn,
                        ModifiedBy = ds.ModifiedBy,
                        ModifiedOn = ds.ModifiedOn
                    },
                    wf =>
                        wf.Company == Company
                        && (
                            Form.Data.Keyword == string.Empty 
                            || wf.Code != null && wf.Code.ToUpper().Contains(Form.Data.Keyword.ToUpper())
                            || wf.Description != null && wf.Description.ToUpper().Contains(Form.Data.Keyword.ToUpper())
                        ),
                    p => p.Code,
                    Form.CurrentPage,
                    Form.ItemsPerPage
                );
    }
}
