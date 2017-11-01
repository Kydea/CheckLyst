using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.DataEngine;
using CheckListManager.WorkFlows.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.WorkFlows.Logic.List
{
    public class ListService : IListService
    {
        private readonly ICommonRepository _commonRepository;
        private readonly IWorkFlowsRepository _workFlowsRepository;

        public ListService(ICommonRepository commonRepository, IWorkFlowsRepository workFlowsRepository)
        {
            _commonRepository = commonRepository;
            _workFlowsRepository = workFlowsRepository;
        }

        public async Task<PagingResponse<List<ListRow>>> GetDataAsync(long Company, PagingRequest<ListRequest> Form)
            =>
                await _workFlowsRepository.GetWorkFlowsAsync(
                    Company,
                    wf => new ListRow
                    {
                        ID = wf.ID,
                        Description = wf.Description,
                        CreatedBy = wf.CreatedBy,
                        CreatedOn = wf.CreatedOn,
                        ModifiedBy = wf.ModifiedBy,
                        ModifiedOn = wf.ModifiedOn
                    },
                    wf =>
                        wf.Company == Company
                        && (Form.Data.Description == string.Empty || wf.Description != null && wf.Description.ToUpper().Contains(Form.Data.Description.ToUpper())),
                    p => p.Description,
                    Form.CurrentPage,
                    Form.ItemsPerPage
                );
    }
}
