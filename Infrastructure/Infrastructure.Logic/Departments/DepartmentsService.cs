using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Core.Web.Security;
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

        private DepartmentsResponse Validate(DepartmentsRow record, bool IsNew)
        {
            var response = new DepartmentsResponse();
            if (string.IsNullOrWhiteSpace(record.Name))
            {
                response.Fields["Name"] = new Field
                {
                    ValidationMessages = new List<ValidationMessage> {
                        new ValidationMessage {
                            Level = ValidationMessage.LevelError,
                            Message = "Invalid name"
                        }
                    }
                };
                response.Valid = false;
            }
            return response;
        }

        public async Task<DepartmentsResponse> AddOrEdit(IDefaultIdentity identity, DepartmentsRow record, bool isNew)
        {
            var response = Validate(record, isNew);
            if (response.Valid)
                try
                {
                    var Department = new Common.Data.Departments
                    {
                        Name = record.Name
                    };
                    if (isNew)
                        await _infrastructureRepository.AddDepartmentAsync(identity, Department);
                    else
                    {
                        Department.ID = record.ID.Value;
                        await _infrastructureRepository.EditDepartmentAsync(identity, Department);
                    }
                }
                catch (Exception Exception)
                {
                    response.ValidationMessages.Add(new ValidationMessage
                    {
                        Level = ValidationMessage.LevelError,
                        Message = Exception.Message
                    });
                    response.Valid = false;
                }

            return response;
        }

        public async Task<DepartmentsResponse> Delete(IDefaultIdentity identity, long id)
        {
            var response = new DepartmentsResponse();
            try
            {
                await _infrastructureRepository.DeleteDepartmentAsync(identity, id);
            }
            catch (Exception Exception)
            {
                response.ValidationMessages.Add(new ValidationMessage
                {
                    Level = ValidationMessage.LevelError,
                    Message = Exception.Message
                });
                response.Valid = false;
            }

            return response;
        }
    }
}
