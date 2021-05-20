using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Reports.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CheckListManager.Reports.Logic.List
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
                        Class = ds.Class,
                        Parameters = ds.Data,
                        CreatedBy = ds.CreatedBy,
                        CreatedOn = ds.CreatedOn,
                        ModifiedBy = ds.ModifiedBy,
                        ModifiedOn = ds.ModifiedOn
                    },
                    wf =>
                        wf.Company == Company
                        && (Form.Data.Code == string.Empty || wf.Code != null && wf.Code.ToUpper().Contains(Form.Data.Code.ToUpper()))
                        && (Form.Data.Description == string.Empty || wf.Description != null && wf.Description.ToUpper().Contains(Form.Data.Description.ToUpper())),
                    p => p.Code,
                    Form.CurrentPage,
                    Form.ItemsPerPage
                );
    }
}
