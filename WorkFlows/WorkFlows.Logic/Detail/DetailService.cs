using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.WorkFlows.Data;
using CheckListManager.Common.Logic.DataEngine;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Queue;
using CheckListManager.Common.Logic.Services;
using CheckListManager.Common.Logic.Trail;
using Newtonsoft.Json;

namespace CheckListManager.WorkFlows.Logic.Detail
{
    public class DetailService : IDetailService
    {
        private readonly IWorkFlowsRepository _workFlowsRepository;
        private readonly ISettingService _settingService;
        private readonly IQueueService _queueService;

        public DetailService(
            IWorkFlowsRepository workFlowsRepository,
            ISettingService settingService,
            IQueueService queueService
        )
        {
            _workFlowsRepository = workFlowsRepository;
            _settingService = settingService;
            _queueService = queueService;
        }

        private async Task<DetailResponse> Validate(IDefaultIdentity identity, DetailRequest record)
        {
            var requireUserReason = await _settingService.RequireUserReasonAsync(identity.CompanyId);
            
            var response = new DetailResponse();
            bool Valid = true;
            if (string.IsNullOrWhiteSpace(record.Description))
                response.Fields["Description"] = new Field
                {
                    ValidationMessages = new List<ValidationMessage> {
                        new ValidationMessage {
                            Level = ValidationMessage.LevelError,
                            Message = "Invalid description"
                        }
                    }
                };

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

            if (record.Steps == null || !record.Steps.Any())
            {
                Valid = false;
                response.ValidationMessages.Add(new ValidationMessage
                {
                    Level = ValidationMessage.LevelError,
                    Message = "The workflow myst have at least one step"
                });
            }
            else if (record.Steps != null)
                for (var Index = 0; Index < record.Steps.Count(); ++Index)
                    if (string.IsNullOrWhiteSpace(record.Steps[Index].Reason))
                        response.Fields[$"Reason{Index}"] = new Field
                        {
                            ValidationMessages = new List<ValidationMessage> {
                                new ValidationMessage {
                                    Level = ValidationMessage.LevelError,
                                    Message = "Mandatory field"
                                }
                            }
                        };

            response.Valid = Valid && !response.Fields.Any();
            return response;
        }

        public async Task<DetailResponse> LoadAsync(IDefaultIdentity identity, DetailRequest record)
        {
            var response = new DetailResponse();
            try
            {
                var WorkFlow = await _workFlowsRepository.GetWorkFlowAsync(identity, record.ID.Value);
                response.Steps = WorkFlow
                    .WorkFlowSteps
                    .GroupBy(s => s.Step)
                    .OrderBy(g => g.Key)
                    .Select(g => new Step
                    {
                        Reason = g.Select(s => s.Reason).FirstOrDefault(),
                        Signatures = g.Select(s => s.WorkFlowStepTags.Select(t => t.Tag).ToArray()).ToArray()
                    })
                    .ToArray();
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

        private List<WorkFlowSteps> BuildSteps(IDefaultIdentity identity, DetailRequest record)
        {
            var List = new List<WorkFlowSteps>();
            for (int Index = 0; Index < record.Steps.Count(); ++Index)
                foreach (var Signature in record.Steps[Index].Signatures)
                    List.Add(new WorkFlowSteps
                    {
                        Reason = record.Steps[Index].Reason,
                        CreatedOn = DateTimeOffset.UtcNow,
                        CreatedBy = identity.Code,
                        Step = Index,
                        WorkFlowStepTags = Signature.Select(s => new WorkFlowStepTags
                        {
                            Tag = s
                        }).ToList()
                    });
            return List;
        }

        public async Task<DetailResponse> AddAsync(IDefaultIdentity identity, DetailRequest record)
        {
            var response = await Validate(identity, record);
            if (response.Valid)
                try
                {
                    var workFlow = await _workFlowsRepository.AddWorkFlowAsync(identity, new Common.Data.WorkFlows
                    {
                        Description = record.Description,
                        WorkFlowSteps = BuildSteps(identity, record)
                    });
                    record.ID = workFlow.ID;
                    await AuditAsync(identity, TrailAction.WorkflowCreated, record);
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

        public async Task<DetailResponse> EditAsync(IDefaultIdentity identity, DetailRequest record)
        {
            var response = await Validate(identity, record);
            if (response.Valid)
                try
                {
                    await _workFlowsRepository.EditWorkFlowAsync(identity, new Common.Data.WorkFlows
                    {
                        ID = record.ID.Value,
                        Description = record.Description,
                        WorkFlowSteps = BuildSteps(identity, record)
                    });
                    await AuditAsync(identity, TrailAction.WorkflowUpdated, record);
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

        public async Task<DetailResponse> DeleteAsync(IDefaultIdentity identity, DetailRequest record)
        {
            var response = new DetailResponse();
            try
            {
                await _workFlowsRepository.DeleteWorkFlowAsync(identity, record.ID.Value);
                await AuditAsync(identity, TrailAction.WorkflowDeleted, record.ID.Value, record.Reason);
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

        private Task AuditAsync(IDefaultIdentity identity, string action, DetailRequest record)
        {
            var information = new
            {
                record.ID,
                record.Description,
                record.Reason,
                record.Steps
            };
            return AuditAsync(identity, action, information);
        }

        private Task AuditAsync(IDefaultIdentity identity, string action, long id, string reason)
        {
            var information = new
            {
                ID = id,
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
