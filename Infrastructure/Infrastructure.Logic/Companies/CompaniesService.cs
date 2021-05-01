using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Cache;
using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Data;
using CheckListManager.Common.Data.ListItems;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.DataEngine;
using CheckListManager.Common.Logic.Queue;
using CheckListManager.Common.Logic.Services;
using CheckListManager.Common.Logic.Trail;
using CheckListManager.Infrastructure.Data;
using Newtonsoft.Json;

namespace CheckListManager.Infrastructure.Logic.Companies
{
    public class CompaniesService : ICompaniesService
    {
        private readonly ICommonRepository _commonRepository;
        private readonly ISettingService _settingService;
        private readonly IConfigurationService _configurationService;
        private readonly IInfrastructureRepository _infrastructureRepository;
        private readonly IQueueService _queueService;
        private readonly IHashService _hashService;

        public CompaniesService(ICommonRepository commonRepository, ISettingService settingService, IConfigurationService configurationService, IInfrastructureRepository infrastructureRepository, IQueueService queueService, IHashService hashService)
        {
            _commonRepository = commonRepository;
            _settingService = settingService;
            _configurationService = configurationService;
            _infrastructureRepository = infrastructureRepository;
            _queueService = queueService;
            _hashService = hashService;
        }

        public async Task<List<Company>> GetSyntheticDataAsync() =>
            await _commonRepository.GetCompaniesAsync();

        public async Task<CompanySettingsResponse> GetSettingsAsync(long companyId)
        {
            var response = new CompanySettingsResponse
            {
                RequireUserReason = await _settingService.RequireUserReasonAsync(companyId),
                InactivityTimeoutSeconds = await _settingService.InactivityTimeoutSecondsAsync(companyId),
                WebApplicationUrl = _configurationService.WebApplicationUrl
            };

            return response;
        }

        public async Task<CompaniesResponse> BindAsync(IDefaultIdentity identity, CompaniesRequest Form)
        {
            CompaniesResponse response = new CompaniesResponse();
            if (Form.Source == null)
            {
                var TimeZones = (await _infrastructureRepository.GetTimeZonesAsync()).Select(tz => new Option(tz, tz));
                response.Fields["TimeZone"] = new Field
                {
                    Options = TimeZones
                };

                response.Fields["Name"] = new Field { Value = Form.Name };
            }

            return response;
        }

        public async Task<PagingResponse<List<CompaniesRow>>> GetDataAsync(IDefaultIdentity identity, PagingRequest<CompaniesRequest> Form)
            => await _infrastructureRepository
                .GetCompaniesAsync(
                    identity,
                    c => new CompaniesRow
                    {
                        ID = c.ID,
                        Name = c.Name,
                        TimeZone = c.TimeZone,
                        CreatedBy = c.CreatedBy,
                        CreatedOn = c.CreatedOn,
                        ModifiedBy = c.ModifiedBy,
                        ModifiedOn = c.ModifiedOn
                    },
                    Form.Data.Name,
                    Form.CurrentPage, Form.ItemsPerPage
                );

        private async Task<CompaniesResponse> ValidateAsync(IDefaultIdentity identity, CompaniesRequest record)
        {
            var requireUserReason = await _settingService.RequireUserReasonAsync(identity.CompanyId);

            var response = new CompaniesResponse();
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

            if (string.IsNullOrWhiteSpace(record.TimeZone))
                response.Fields["TimeZone"] = new Field
                {
                    ValidationMessages = new List<ValidationMessage> {
                        new ValidationMessage {
                            Level = ValidationMessage.LevelError,
                            Message = "Invalid time zone"
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

        public async Task<CompaniesResponse> AddAsync(IDefaultIdentity identity, CompaniesRequest record)
        {
            if (identity.CompanyId != 1) throw new UnauthorizedAccessException();

            var response = await ValidateAsync(identity, record);
            if (response.Valid)
                try
                {
                    var company = await _infrastructureRepository.AddCompanyAsync(identity, new Common.Data.Companies
                    {
                        Name = record.Name,
                        TimeZone = record.TimeZone,
                        CreatedOn = DateTimeOffset.Now,
                        CreatedBy = identity.Code
                    }, new Users
                    {
                        Code = record.Username,
                        Description = "(Configurator)",
                        Password = _hashService.HashStringToBytes(record.Password),
                        CreatedOn = DateTimeOffset.Now,
                        Enabled = true
                    });
                    record.ID = company.ID;
                    await AuditAsync(identity, TrailAction.CompanyCreated, new
                    {
                        record.ID,
                        record.Name,
                        record.TimeZone,
                        record.Username,
                        ReasonForChange = record.Reason,
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

        public async Task<CompaniesResponse> EditAsync(IDefaultIdentity identity, CompaniesRequest record)
        {
            if (identity.CompanyId != record.ID) throw new UnauthorizedAccessException();

            var response = await ValidateAsync(identity, record);
            if (response.Valid)
                try
                {
                    await _infrastructureRepository.EditCompanyAsync(identity, new Common.Data.Companies
                    {
                        ID = record.ID.Value,
                        Name = record.Name,
                        TimeZone = record.TimeZone
                    });
                    await AuditAsync(identity, TrailAction.CompanyUpdated, new
                    {
                        record.ID,
                        record.Name,
                        record.TimeZone,
                        ReasonForChange = record.Reason,
                    });
                    _settingService.Clear(record.ID.Value);
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

        public async Task<CompaniesResponse> LoadAsync(IDefaultIdentity identity, CompaniesRequest record)
        {
            if (identity.CompanyId != 1 && identity.CompanyId != record.ID) throw new UnauthorizedAccessException();
            var settings = await _infrastructureRepository.GetSettings(identity, record.ID);
            return new CompaniesResponse
            {
                Settings = settings
            };
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
