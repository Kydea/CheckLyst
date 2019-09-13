using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.DataEngine;
using CheckListManager.Common.Logic.Queue;
using CheckListManager.Common.Logic.Services;
using CheckListManager.Common.Logic.Trail;
using CheckListManager.Infrastructure.Data;
using Newtonsoft.Json;

namespace CheckListManager.Infrastructure.Logic.Departments
{
    public class DepartmentsService : IDepartmentsService
    {
        private readonly IInfrastructureRepository _infrastructureRepository;
        private readonly ISettingService _settingService;
        private readonly IQueueService _queueService;

        public DepartmentsService(
            IInfrastructureRepository infrastructureRepository,
            ISettingService settingService,
            IQueueService queueService
        )
        {
            _infrastructureRepository = infrastructureRepository;
            _settingService = settingService;
            _queueService = queueService;
        }

        public async Task<PagingResponse<List<DepartmentsRow>>> GetDataAsync(IDefaultIdentity identity, PagingRequest<DepartmentsRequest> Form)
            => await _infrastructureRepository
                .GetDepartmentsAsync(
                    identity,
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

        private async Task<DepartmentsResponse> ValidateAsync(IDefaultIdentity identity, DepartmentsRow record)
        {
            var requireUserReason = await _settingService.RequireUserReasonAsync(identity.CompanyId);
            
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

            if (requireUserReason && string.IsNullOrEmpty(record.Reason))
            {
                response.Fields["Reason"] = new Field
                {
                    ValidationMessages = new List<ValidationMessage>
                    {
                        new ValidationMessage
                        {
                            Level = ValidationMessage.LevelError,
                            Message = "Reason is a required field"
                        }
                    }
                };
            }
            
            response.Valid = !response.Fields.Any();
            return response;
        }

        public async Task<DepartmentsResponse> AddOrEdit(IDefaultIdentity identity, DepartmentsRow record, bool isNew)
        {
            var response = await ValidateAsync(identity, record);
            if (response.Valid)
                try
                {
                    var Department = new Common.Data.Departments
                    {
                        Name = record.Name
                    };
                    if (isNew)
                    {
                        await _infrastructureRepository.AddDepartmentAsync(identity, Department);
                        record.ID = Department.ID;
                        await AuditAsync(identity, TrailAction.DepartmentCreated, record);
                    }
                    else
                    {
                        Department.ID = record.ID.Value;
                        await _infrastructureRepository.EditDepartmentAsync(identity, Department);
                        await AuditAsync(identity, TrailAction.DepartmentUpdated, record);
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

        public async Task<DepartmentsResponse> Delete(IDefaultIdentity identity, DepartmentsRow record)
        {
            var response = new DepartmentsResponse();
            try
            {
                await _infrastructureRepository.DeleteDepartmentAsync(identity, record.ID.Value);
                await AuditAsync(identity, TrailAction.DepartmentDeleted, record.ID.Value, record.Reason);
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

        private Task AuditAsync(IDefaultIdentity identity, string action, DepartmentsRow record)
        {
            var information = new
            {
                record.ID,
                record.Name,
                record.Reason,
            };
            return AuditAsync(identity, action, information);
        }

        private Task AuditAsync(IDefaultIdentity identity, string action, long id, string reason)
        {
            var information = new
            {
                Id = id,
                Reason = reason
            };
            return AuditAsync(identity, action, information);
        }

        private Task AuditAsync(IDefaultIdentity identity, string action, object information)
        {
            return _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
            {
                Company = identity.CompanyId,
                User = identity.Code,
                Action = action,
                Date = DateTimeOffset.UtcNow,
                Information = information.ToJson(new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None })
            });
        }        
    }
}
