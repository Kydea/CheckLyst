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

namespace CheckListManager.Infrastructure.Logic.Departments
{
    public class DepartmentsService : IDepartmentsService
    {
        private readonly IInfrastructureRepository _infrastructureRepository;
        private readonly ICommonRepository _commonRepository;

        public DepartmentsService(IInfrastructureRepository infrastructureRepository, ICommonRepository commonRepository)
        {
            _infrastructureRepository = infrastructureRepository;
            _commonRepository = commonRepository;
        }

        public async Task<PagingResponse<List<DepartmentsRow>>> GetDataAsync(long Company, PagingRequest<DepartmentsRequest> Form)
            => await _infrastructureRepository
                .GetDepartmentsAsync(
                    Company,
                    d => new DepartmentsRow
                    {
                        ID = d.ID,
                        Name = d.Name,
                        CreatedBy = d.CreatedBy,
                        CreatedOn = d.CreatedOn,
                        ModifiedBy = d.ModifiedBy,
                        ModifiedOn = d.ModifiedOn
                    },
                    Form.Data.Department, Form.CurrentPage, Form.ItemsPerPage
                );

        public async Task<DepartmentsResponse> Edit(long Company, DepartmentsRow record)
        {
            var response = new DepartmentsResponse();
            Common.Data.Departments Department;
            if (record.ID.HasValue) {
                Department = await _infrastructureRepository.GetDepartmentAsync(Company, record.ID.Value);
                Department.ModifiedOn = DateTimeOffset.UtcNow;
                //Department.ModifiedBy = //TODO current user
            }
            else
            {
                Department = new Common.Data.Departments
                {
                    CreatedOn = DateTimeOffset.UtcNow,
                    //CreatedBy = //TODO current user
                };
            }
            await _infrastructureRepository.EditDepartmentAsync(Company, new Common.Data.Departments
            {
                Name = record.Name;
            });
            return response;
        }
    }
}
