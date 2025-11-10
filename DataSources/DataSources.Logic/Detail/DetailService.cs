using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Logic.DataEngine;
using CheckListManager.DataSources.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Logic.Queue;
using CheckListManager.Common.Logic.Services;
using CheckListManager.Common.Logic.Trail;
using Newtonsoft.Json;
using CheckListManager.Common.Data;

namespace CheckListManager.DataSources.Logic.Detail
{
    public class DetailService : IDetailService
    {
        private readonly IDataSourcesRepository _dataSourcesRepository;
        private readonly ISettingService _settingService;
        private readonly IQueueService _queueService;
        private readonly ICommonRepository _commonRepository;

        public DetailService(
            IDataSourcesRepository dataSourcesRepository,
            ISettingService settingService,
            IQueueService queueService,
            ICommonRepository commonRepository
        )
        {
            _dataSourcesRepository = dataSourcesRepository;
            _settingService = settingService;
            _queueService = queueService;
            _commonRepository = commonRepository;
        }

        private async Task<DetailResponse> ValidateAsync(IDefaultIdentity identity, DetailRequest record)
        {
            var requireUserReason = await _settingService.RequireUserReasonAsync(identity.CompanyId);
            
            var response = new DetailResponse();
            if (string.IsNullOrWhiteSpace(record.Code))
                response.Fields["Code"] = new Field
                {
                    ValidationMessages = new List<ValidationMessage> {
                        new ValidationMessage {
                            Level = ValidationMessage.LevelError,
                            Message = "Invalid code"
                        }
                    }
                };

            if (string.IsNullOrWhiteSpace(record.Class))
                response.Fields["Class"] = new Field
                {
                    ValidationMessages = new List<ValidationMessage> {
                        new ValidationMessage {
                            Level = ValidationMessage.LevelError,
                            Message = "Invalid class"
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

            response.Valid = response.Fields.Count == 0;

            if (response.Valid)
                response.Fields["Code"] = new Field
                {
                    Value = record.Code,
                    Editable = false
                };

            return response;
        }

        public async Task<DetailResponse> AddAsync(IDefaultIdentity identity, DetailRequest record)
        {
            var response = await ValidateAsync(identity, record);
            if (response.Valid)
                try
                {
                    await _dataSourcesRepository.AddDataSourceAsync(identity, new Common.Data.DataSources
                    {
                        Code = record.Code,
                        Description = record.Description,
                        Class = record.Class,
                        Data = record.Parameters,
                        Password = record.Password
                    });
                    await AuditAsync(identity, TrailAction.DataSourceCreated, record);
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
            var response = await ValidateAsync(identity, record);
            if (response.Valid)
                try
                {
                    await _dataSourcesRepository.EditDataSourceAsync(identity, new Common.Data.DataSources
                    {
                        Code = record.Code,
                        Description = record.Description,
                        Class = record.Class,
                        Data = record.Parameters,
                        Password = record.Password
                    });
                    await AuditAsync(identity, TrailAction.DataSourceUpdated, record);
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
                await _dataSourcesRepository.DeleteDataSourceAsync(identity, record.Code);
                await AuditAsync(identity, TrailAction.DataSourceDeleted, record.Code, record.Reason);
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
                record.Code,
                record.Description,
                record.Class,
                record.Parameters,
                Password = new string('*', Math.Min(3, record.Password?.Length ?? 0)), // Avoid to log password from UI
                ReasonForChange = record.Reason
            };
            return _commonRepository.AuditAsync(identity, action, information);
        }

        private Task AuditAsync(IDefaultIdentity identity, string action, string code, string reason)
        {
            var information = new
            {
                Code = code,
                ReasonForChange = reason
            };
            return _commonRepository.AuditAsync(identity, action, information);
        }
    }
}
