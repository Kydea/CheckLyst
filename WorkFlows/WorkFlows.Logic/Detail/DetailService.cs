using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.WorkFlows.Data;
using CheckListManager.Common.Logic.DataEngine;
using CheckListManager.Common.Data;

namespace CheckListManager.WorkFlows.Logic.Detail
{
    public class DetailService : IDetailService
    {
        private readonly IWorkFlowsRepository _workFlowsRepository;

        public DetailService(IWorkFlowsRepository workFlowsRepository)
        {
            _workFlowsRepository = workFlowsRepository;
        }

        private DetailResponse Validate(DetailRequest record)
        {
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

            if (record.Steps != null && record.Steps.Count() == 0)
            {
                Valid = false;
                response.ValidationMessages.Add(new ValidationMessage
                {
                    Level = ValidationMessage.LevelError,
                    Message = "The workflow myst have at least one step"
                });
            }
            else
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

            response.Valid = Valid && response.Fields.Count() == 0;
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
            var response = Validate(record);
            if (response.Valid)
                try
                {
                    await _workFlowsRepository.AddWorkFlowAsync(identity, new Common.Data.WorkFlows
                    {
                        Description = record.Description,
                        WorkFlowSteps = BuildSteps(identity, record)
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

        public async Task<DetailResponse> EditAsync(IDefaultIdentity identity, DetailRequest record)
        {
            var response = Validate(record);
            if (response.Valid)
                try
                {
                    await _workFlowsRepository.EditWorkFlowAsync(identity, new Common.Data.WorkFlows
                    {
                        ID = record.ID.Value,
                        Description = record.Description,
                        WorkFlowSteps = BuildSteps(identity, record)
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

        public async Task<DetailResponse> DeleteAsync(IDefaultIdentity identity, long id)
        {
            var response = new DetailResponse();
            try
            {
                await _workFlowsRepository.DeleteWorkFlowAsync(identity, id);
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
