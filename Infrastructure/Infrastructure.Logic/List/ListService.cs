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

namespace CheckListManager.Infrastructure.Logic.List
{
    public class ListService : IListService
    {
        private readonly IInfrastructureRepository _infrastructureRepository;
        private readonly ICommonRepository _commonRepository;

        public ListService(IInfrastructureRepository infrastructureRepository, ICommonRepository commonRepository)
        {
            _infrastructureRepository = infrastructureRepository;
            _commonRepository = commonRepository;
        }

        public async Task<ListResponse> BindAsync(long Company, ListRequest Form)
        {
            ListResponse response = new ListResponse();
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

                response.Fields["PrototypeType"] = new Field { Value = Form.PrototypeType };
            }

            return response;
        }

        public async Task<PagingResponse<List<ListRow>>> GetDataAsync(long Company, PagingRequest<ListRequest> Form)
            => await _infrastructureRepository
                .GetPrototypeTypesAsync(
                    Company,
                    pt => new ListRow {
                        Department = pt.Departments.Name,
                        ID = pt.ID,
                        Description = pt.Description,
                        ApprovationWorkFlow = pt.ApprovationWorkFlow,
                        ApprovationWorkFlowDescription = pt.WorkFlows.Description,
                        ObsolescenceWorkFlow = pt.ObsolescenceWorkFlow,
                        ObsolescenceWorkFlowDescription = pt.WorkFlows1.Description
                    },
                    Form.Data.Department, Form.Data.PrototypeType, Form.CurrentPage, Form.ItemsPerPage
                );
    }
}
