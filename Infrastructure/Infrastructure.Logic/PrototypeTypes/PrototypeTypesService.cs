using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.DataEngine;
using CheckListManager.Common.Logic.Trail;
using CheckListManager.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Infrastructure.Logic.PrototypeTypes
{
    public class PrototypeTypesService : IPrototypeTypesService
    {
        private readonly IInfrastructureRepository _infrastructureRepository;
        private readonly ICommonRepository _commonRepository;

        public PrototypeTypesService(IInfrastructureRepository infrastructureRepository, ICommonRepository commonRepository)
        {
            _infrastructureRepository = infrastructureRepository;
            _commonRepository = commonRepository;
        }

        public async Task<PrototypeTypesResponse> BindAsync(long Company, PrototypeTypesRequest Form)
        {
            PrototypeTypesResponse response = new PrototypeTypesResponse();
            long? Department = Form.Department;

            if (Form.Source == null)
            {
                //initial load
                var Departments = await _commonRepository.GetDepartmentsAsync(Company);
                Department = Form.Department.HasValue && Departments.Any(d => d.ID == Form.Department) ? Form.Department.Value : Departments.FirstOrDefault()?.ID;
                response.Fields["Department"] = new Field
                {
                    Options = Departments.Select(d => new Option(d.ID.ToString(), d.Name)),
                    Value = Department?.ToString()
                };
                response.Valid &= Departments.Any();

                var WorkFlows = (await _infrastructureRepository.GetWorkFlows(Company)).Select(wf => new Option(wf.ID.ToString(), wf.Description));
                response.Fields["ApprovationWorkFlow"] = new Field
                {
                    Options = WorkFlows
                };

                response.Fields["ObsolescenceWorkFlow"] = new Field
                {
                    Options = WorkFlows
                };

                response.Fields["PrototypeType"] = new Field { Value = Form.PrototypeType };
            }

            return response;
        }

        public async Task<PagingResponse<List<PrototypeTypesRow>>> GetDataAsync(long Company, PagingRequest<PrototypeTypesRequest> Form)
            => await _infrastructureRepository
                .GetPrototypeTypesAsync(
                    Company,
                    pt => new PrototypeTypesRow {
                        Department = pt.Department.Value,
                        ID = pt.ID,
                        Description = pt.Description,
                        ApprovationWorkFlow = pt.ApprovationWorkFlow,
                        ApprovationWorkFlowDescription = pt.WorkFlows.Description,
                        ObsolescenceWorkFlow = pt.ObsolescenceWorkFlow,
                        ObsolescenceWorkFlowDescription = pt.WorkFlows1.Description,
                        CreatedBy = pt.CreatedBy,
                        CreatedOn = pt.CreatedOn,
                        ModifiedBy = pt.ModifiedBy,
                        ModifiedOn = pt.ModifiedOn
                    },
                    Form.Data.Department, Form.Data.PrototypeType, Form.CurrentPage, Form.ItemsPerPage
                );
    }
}
