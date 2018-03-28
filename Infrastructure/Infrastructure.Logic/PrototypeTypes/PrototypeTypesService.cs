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

        public async Task<PrototypeTypesResponse> BindAsync(IDefaultIdentity identity, PrototypeTypesRequest Form)
        {
            PrototypeTypesResponse response = new PrototypeTypesResponse();
            long? Department = Form.Department;

            if (Form.Source == null)
            {
                //initial load
                var Departments = await _commonRepository.GetDepartmentsAsync(identity, null);
                Department = Form.Department.HasValue && Departments.Any(d => d.ID == Form.Department) ? Form.Department.Value : Departments.FirstOrDefault()?.ID;
                response.Fields["Department"] = new Field
                {
                    Options = Departments.OrderBy(x => x.Name).Select(d => new Option(d.ID.ToString(), d.Name)),
                    Value = Department?.ToString()
                };
                response.Valid &= Departments.Any();

                var WorkFlows = (await _infrastructureRepository.GetWorkFlows(identity.CompanyId)).Select(wf => new Option(wf.ID.ToString(), wf.Description));
                response.Fields["ApprovationWorkFlow"] = new Field
                {
                    Options = WorkFlows
                };

                response.Fields["ObsolescenceWorkFlow"] = new Field
                {
                    Options = WorkFlows
                };

                response.Fields["PrototypeType"] = new Field { Value = Form.Name };
            }

            return response;
        }

        public async Task<PagingResponse<List<PrototypeTypesRow>>> GetDataAsync(IDefaultIdentity identity, PagingRequest<PrototypeTypesRequest> Form)
            => await _infrastructureRepository
                .GetPrototypeTypesAsync(
                    identity,
                    pt => new PrototypeTypesRow
                    {
                        Department = pt.Department,
                        DepartmentName = pt.Departments.Name,
                        ID = pt.ID,
                        Name = pt.Name,
                        ApprovationWorkFlow = pt.ApprovationWorkFlow,
                        ApprovationWorkFlowDescription = pt.WorkFlows.Description,
                        ObsolescenceWorkFlow = pt.ObsolescenceWorkFlow,
                        ObsolescenceWorkFlowDescription = pt.WorkFlows1.Description,
                        CreatedBy = pt.CreatedBy,
                        CreatedOn = pt.CreatedOn,
                        ModifiedBy = pt.ModifiedBy,
                        ModifiedOn = pt.ModifiedOn
                    },
                    Form.Data.Department, Form.Data.Name, Form.Data.FunctionGroup, 
                    Form.CurrentPage, Form.ItemsPerPage
                );

        public async Task<PagingResponse<List<PrototypeTypesSyntheticRow>>> GetSyntheticDataAsync(IDefaultIdentity identity, PagingRequest<PrototypeTypesRequest> Form)
            => await _infrastructureRepository
                .GetPrototypeTypesAsync(
                    identity,
                    pt => new PrototypeTypesSyntheticRow
                    {
                        Department = pt.Department,
                        DepartmentName = pt.Departments.Name,
                        ID = pt.ID,
                        Name = pt.Name
                    },
                    Form.Data.Department, Form.Data.Name, Form.Data.FunctionGroup,
                    Form.CurrentPage, Form.ItemsPerPage,
                    Form.Data.Function
                );

        private PrototypeTypesResponse Validate(PrototypeTypesRequest record)
        {
            var response = new PrototypeTypesResponse();
            if (string.IsNullOrWhiteSpace(record.Name))
                response.Fields["Name"] = new Field
                {
                    ValidationMessages = new List<ValidationMessage> {
                        new ValidationMessage {
                            Level = ValidationMessage.LevelError,
                            Message = "Invalid name"
                        }
                    }
                };

            if (!record.ApprovationWorkFlow.HasValue)
                response.Fields["ApprovationWorkFlow"] = new Field
                {
                    ValidationMessages = new List<ValidationMessage> {
                        new ValidationMessage {
                            Level = ValidationMessage.LevelError,
                            Message = "Invalid approval workflow"
                        }
                    }
                };

            if (!record.ObsolescenceWorkFlow.HasValue)
                response.Fields["ObsolescenceWorkFlow"] = new Field
                {
                    ValidationMessages = new List<ValidationMessage> {
                        new ValidationMessage {
                            Level = ValidationMessage.LevelError,
                            Message = "Invalid obsolescence workflow"
                        }
                    }
                };

            response.Valid = response.Fields.Count() == 0;
            return response;
        }

        public async Task<PrototypeTypesResponse> Add(IDefaultIdentity identity, PrototypeTypesRequest record)
        {
            var response = Validate(record);
            if (response.Valid)
                try
                {
                    await _infrastructureRepository.AddPrototypeTypeAsync(identity, new Common.Data.PrototypeTypes
                    {
                        Department = record.Department.Value,
                        Name = record.Name,
                        ApprovationWorkFlow = record.ApprovationWorkFlow,
                        ObsolescenceWorkFlow = record.ObsolescenceWorkFlow
                    });
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

        public async Task<PrototypeTypesResponse> Edit(IDefaultIdentity identity, PrototypeTypesRequest record)
        {
            var response = Validate(record);
            if (response.Valid)
                try
                {
                    await _infrastructureRepository.EditPrototypeTypeAsync(identity, new Common.Data.PrototypeTypes
                    {
                        ID = record.ID.Value,
                        Department = record.Department.Value,
                        Name = record.Name,
                        ApprovationWorkFlow = record.ApprovationWorkFlow,
                        ObsolescenceWorkFlow = record.ObsolescenceWorkFlow
                    });
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

        public async Task<PrototypeTypesResponse> Delete(IDefaultIdentity identity, long id)
        {
            var response = new PrototypeTypesResponse();
            try
            {
                await _infrastructureRepository.DeletePrototypeTypeAsync(identity, id);
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