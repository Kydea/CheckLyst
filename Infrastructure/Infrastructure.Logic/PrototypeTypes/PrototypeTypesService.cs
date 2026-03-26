using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.DataEngine;
using CheckListManager.Common.Logic.License;
using CheckListManager.Common.Logic.Queue;
using CheckListManager.Common.Logic.Services;
using CheckListManager.Common.Logic.Trail;
using CheckListManager.Infrastructure.Data;
using CheckListManager.Infrastructure.Data.ListItems;
using Newtonsoft.Json;

namespace CheckListManager.Infrastructure.Logic.PrototypeTypes
{
    public class PrototypeTypesService : IPrototypeTypesService
    {
        private readonly IInfrastructureRepository _infrastructureRepository;
        private readonly ICommonRepository _commonRepository;
        private readonly ISettingService _settingService;
        private readonly IQueueService _queueService;

        public PrototypeTypesService(
            IInfrastructureRepository infrastructureRepository,
            ICommonRepository commonRepository,
            ISettingService settingService,
            IQueueService queueService
        )
        {
            _infrastructureRepository = infrastructureRepository;
            _commonRepository = commonRepository;
            _settingService = settingService;
            _queueService = queueService;
        }

        public async Task<PrototypeTypesResponse> BindAsync(IDefaultIdentity identity, PrototypeTypesRequest Form)
        {
            PrototypeTypesResponse response = new PrototypeTypesResponse();
            long? Department = Form.Department;

            if (Form.Source == null)
            {
                //initial load
                var Departments = await _commonRepository.GetDepartmentsAsync(identity.CompanyId);
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

                var FTPServers = (await _infrastructureRepository.GetFTPServersAsync(identity, ftps => new { ftps.ID, ftps.Name }, string.Empty, string.Empty, 1, 1000)).Data.Select(ftps => new Option(ftps.ID.ToString(), ftps.Name)).ToList();
                FTPServers.Insert(0, new Option("-1", "(default)"));
                response.Fields["PrototypesFTPServer"] = new Field
                {
                    Options = FTPServers
                };

                response.Fields["InstancesFTPServer"] = new Field
                {
                    Options = FTPServers
                };

                response.Fields["Name"] = new Field { Value = Form.Name };
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
                        LifeCycle = pt.LifeCycle,
                        ApprovationWorkFlow = pt.ApprovationWorkFlow,
                        ApprovationWorkFlowDescription = pt.WorkFlows.Description,
                        ObsolescenceWorkFlow = pt.ObsolescenceWorkFlow,
                        ObsolescenceWorkFlowDescription = pt.WorkFlows1.Description,
                        PrototypesFTPServer = !pt.PrototypesFTPServerEnabled ? default(long?) : (pt.PrototypesFTPServer ?? -1),
                        InstancesFTPServer = !pt.InstancesFTPServerEnabled ? default(long?) : (pt.InstancesFTPServer ?? -1),
                        CreatedBy = pt.CreatedBy,
                        CreatedOn = pt.CreatedOn,
                        ModifiedBy = pt.ModifiedBy,
                        ModifiedOn = pt.ModifiedOn
                    },
                    Form.Data.Department, Form.Data.Name, Form.CurrentPage, Form.ItemsPerPage
                );

        public async Task<List<PrototypeTypesSyntheticRow>> GetSyntheticDataAsync(IDefaultIdentity identity, PrototypeTypesRequest Form)
            => (
                await _commonRepository
                .GetPrototypeTypesAsync(identity, -1, Form?.FunctionGroup, Form?.Function)
            )
                .Select(pt => new PrototypeTypesSyntheticRow { ID = pt.ID, Name = pt.Name, Department = pt.Department, DepartmentName = pt.DepartmentName, LifeCycle = pt.LifeCycle }).ToList();

        private async Task<PrototypeTypesResponse> ValidateAsync(IDefaultIdentity identity, PrototypeTypesRequest record)
        {
            var requireUserReason = await _settingService.RequireUserReasonAsync(identity.CompanyId);
            
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
                    Fixed = true,
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
                    Fixed = true,
                    ValidationMessages = new List<ValidationMessage> {
                        new ValidationMessage {
                            Level = ValidationMessage.LevelError,
                            Message = "Invalid obsolescence workflow"
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

            response.Valid = !response.Fields.Any();
            return response;
        }

        public async Task<PrototypeTypesResponse> Add(IDefaultIdentity identity, PrototypeTypesRequest record)
        {
            var licensedCount = (await _settingService.LicenseAsync(identity.CompanyId)).Payload.PrototypeTypesCount;
            if (licensedCount.HasValue)
            {
                var currentCount = await _infrastructureRepository.CountPrototypeTypesAsync(identity.CompanyId);
                if (licensedCount.Value <= currentCount)
                    throw new LicenseException("The maximum number of prototype types allowed by the license has been reached");
            }

            var response = await ValidateAsync(identity, record);
            if (response.Valid)
                try
                {
                    var prototypeType = await _infrastructureRepository.AddPrototypeTypeAsync(identity, new Common.Data.PrototypeTypes
                    {
                        Department = record.Department.Value,
                        Name = record.Name,
                        LifeCycle = record.LifeCycle,
                        ApprovationWorkFlow = record.ApprovationWorkFlow,
                        ObsolescenceWorkFlow = record.ObsolescenceWorkFlow,
                        PrototypesFTPServerEnabled = record.PrototypesFTPServer.HasValue,
                        PrototypesFTPServer = record.PrototypesFTPServer >= 0 ? record.PrototypesFTPServer : default,
                        InstancesFTPServerEnabled = record.InstancesFTPServer.HasValue,
                        InstancesFTPServer = record.InstancesFTPServer >= 0 ? record.InstancesFTPServer : default
                    });
                    record.ID = prototypeType.ID;
                    await AuditAsync(identity, TrailAction.PrototypeTypeCreated, record);
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
            var response = await ValidateAsync(identity, record);
            if (response.Valid)
                try
                {
                    await _infrastructureRepository.EditPrototypeTypeAsync(identity, new Common.Data.PrototypeTypes
                    {
                        ID = record.ID.Value,
                        Department = record.Department.Value,
                        Name = record.Name,
                        LifeCycle = record.LifeCycle,
                        ApprovationWorkFlow = record.ApprovationWorkFlow,
                        ObsolescenceWorkFlow = record.ObsolescenceWorkFlow,
                        PrototypesFTPServerEnabled = record.PrototypesFTPServer.HasValue,
                        PrototypesFTPServer = record.PrototypesFTPServer >= 0 ? record.PrototypesFTPServer : default,
                        InstancesFTPServerEnabled = record.InstancesFTPServer.HasValue,
                        InstancesFTPServer = record.InstancesFTPServer >= 0 ? record.InstancesFTPServer : default
                    });
                    await AuditAsync(identity, TrailAction.PrototypeTypeUpdated, record);
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

        public async Task<PrototypeTypesResponse> Delete(IDefaultIdentity identity, PrototypeTypesRequest record)
        {
            var response = new PrototypeTypesResponse();
            try
            {
                await _infrastructureRepository.DeletePrototypeTypeAsync(identity, record.ID.Value);
                await AuditAsync(identity, TrailAction.PrototypeTypeDeleted, record.ID.Value, record.Reason);
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

        private Task AuditAsync(IDefaultIdentity identity, string action, PrototypeTypesRequest record)
        {
            var information = new
            {
                record.ID,
                record.Name,
                record.LifeCycle,
                record.Department,
                record.ApprovationWorkFlow,
                record.ObsolescenceWorkFlow,
                record.PrototypesFTPServer,
                record.InstancesFTPServer,
                ReasonForChange = record.Reason,
            };
            return _commonRepository.AuditAsync(identity, action, information);
        }

        private Task AuditAsync(IDefaultIdentity identity, string action, long id, string reason)
        {
            var information = new
            {
                ID = id,
                ReasonForChange = reason
            };
            return _commonRepository.AuditAsync(identity, action, information);
        }
    }
}