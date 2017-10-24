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

        public async Task<ListResponse> BindAsync(long Company, ListRequest Form)
        {
            ListResponse response = new ListResponse();
            long? Department = Form.Department;

            if (Form.Source == null)
            {
                //initial load

                response.Fields["Name"] = new Field { Value = Form.Name };

                var Departments = await _commonRepository.GetDepartmentsAsync(Company);
                Department = Form.Department.HasValue && Departments.Any(d => d.ID == Form.Department) ? Form.Department.Value : Departments.FirstOrDefault()?.ID;
                response.Fields["Department"] = new Field
                {
                    Options = Departments.Select(d => new Option(d.ID.ToString(), d.Name)),
                    Value = Department?.ToString()
                };
                response.Valid &= Departments.Any();

                var States = new List<Option>() { new Option(null, "All") };
                States.AddRange((await _commonRepository.GetVersionStatesAsync()).Select(x => new Option(x)));
                response.Fields["State"] = new Field
                {
                    Options = States,
                    Value = Form.State
                };
            }

            if (Form.Source == null || Form.Source == "department")
            {
                var PrototypeTypes = await _commonRepository.GetPrototypeTypesAsync(Department ?? -1);
                response.Fields["PrototypeType"] = new Field
                {
                    Options = PrototypeTypes.Select(d => new Option(d.ID.ToString(), d.Description)),
                    Value = Form.PrototypeType?.ToString()
                };
                response.Valid &= PrototypeTypes.Any();

            }

            return response;
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
                        && (Form.Data.Name == string.Empty || wf.Description != null && wf.Description.ToUpper().Contains(Form.Data.Name.ToUpper())),
                    p => p.Description,
                    Form.CurrentPage,
                    Form.ItemsPerPage
                );
    }
}
