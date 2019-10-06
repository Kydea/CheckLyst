using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.DataEngine;
using CheckListManager.Common.Logic.Queue;
using CheckListManager.Common.Logic.Services;
using CheckListManager.Common.Logic.Terminal.FTP;
using CheckListManager.Common.Logic.Trail;
using CheckListManager.Infrastructure.Data;
using Newtonsoft.Json;

namespace CheckListManager.Infrastructure.Logic.FTPServers
{
    public class FTPServersService : IFTPServersService
    {
        private readonly IInfrastructureRepository _infrastructureRepository;
        private readonly ISettingService _settingService;
        private readonly IQueueService _queueService;

        private readonly Func<Common.Data.FTPServers, FTPServersRow> _mapperFunc;
        private readonly Expression<Func<Common.Data.FTPServers, FTPServersRow>> _mapper = d => new FTPServersRow
        {
            ID = d.ID,
            Name = d.Name,
            Host = d.Host,
            Port = d.Port,
            Folder = d.Folder,
            UserName = d.UserName,
            Password = d.Password,
            SSL = d.SSL,
            Passive = d.Passive,
            PrototypesDefault = d.PrototypesDefault,
            InstancesDefault = d.InstancesDefault,

            CreatedBy = d.CreatedBy,
            CreatedOn = d.CreatedOn,
            ModifiedBy = d.ModifiedBy,
            ModifiedOn = d.ModifiedOn
        };

        public FTPServersService(
            IInfrastructureRepository infrastructureRepository,
            ISettingService settingService,
            IQueueService queueService
        )
        {
            _infrastructureRepository = infrastructureRepository;
            _settingService = settingService;
            _queueService = queueService;

            _mapperFunc = _mapper.Compile();
        }

        public async Task<PagingResponse<List<FTPServersRow>>> GetDataAsync(IDefaultIdentity identity, PagingRequest<FTPServersRequest> Form)
            => await _infrastructureRepository
                .GetFTPServersAsync(
                    identity,
                    _mapper,
                    Form.Data.Name, Form.Data.Host, Form.CurrentPage, Form.ItemsPerPage
                );

        private async Task<FTPServersResponse> ValidateAsync(IDefaultIdentity identity, FTPServersRow record)
        {
            var requireUserReason = await _settingService.RequireUserReasonAsync(identity.CompanyId);
            
            var response = new FTPServersResponse();
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
            if (string.IsNullOrWhiteSpace(record.Host))
            {
                response.Fields["Host"] = new Field
                {
                    ValidationMessages = new List<ValidationMessage> {
                        new ValidationMessage {
                            Level = ValidationMessage.LevelError,
                            Message = "Invalid host"
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

        public async Task<FTPServersResponse> AddOrEdit(IDefaultIdentity identity, FTPServersRow record, bool isNew)
        {
            var response = await ValidateAsync(identity, record);
            if (response.Valid)
                try
                {
                    record.Port = record.Port ?? 21;
                    var FTPServer = new Common.Data.FTPServers
                    {
                        Name = record.Name,
                        Host = record.Host,
                        Port = record.Port.Value,
                        Folder = record.Folder,
                        UserName = record.UserName,
                        Password = record.Password,
                        Passive = record.Passive,
                        SSL = record.SSL,
                        PrototypesDefault = record.PrototypesDefault,
                        InstancesDefault = record.InstancesDefault
                    };
                    if (isNew)
                    {
                        await _infrastructureRepository.AddFTPServerAsync(identity, FTPServer);
                        record.ID = FTPServer.ID;
                        response.Data = _mapperFunc(FTPServer);
                        await AuditAsync(identity, TrailAction.FtpServerCreated, record);
                    }
                    else
                    {
                        FTPServer.ID = record.ID.Value;
                        await _infrastructureRepository.EditFTPServerAsync(identity, FTPServer);
                        response.Data = _mapperFunc(FTPServer);
                        await AuditAsync(identity, TrailAction.FtpServerUpdated, record);
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

        public async Task<FTPServersResponse> Delete(IDefaultIdentity identity, FTPServersRow record)
        {
            var response = new FTPServersResponse();
            try
            {
                await _infrastructureRepository.DeleteFTPServerAsync(identity, record.ID.Value);
                await AuditAsync(identity, TrailAction.FtpServerDeleted, record.ID.Value, record.Reason);
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

        public async Task<string> Test(IDefaultIdentity identity, long id)
        {
            var record = await _infrastructureRepository.GetFTPServerAsync(identity, id);

            var result = await new FTPClient().TestAsync(new FTPConnectionSettings {
                Host = record.Host,
                Folder = record.Folder,
                Port = record.Port,
                EnableSSL = record.SSL,
                UsePassive = record.Passive,
                UserName = record.UserName,
                Password = record.Password
            });

            return result;
        }
        
        private Task AuditAsync(IDefaultIdentity identity, string action, FTPServersRow record)
        {
            var information = new
            {
                record.ID,
                record.Name,
                record.Host,
                record.Port,
                record.Folder,
                record.UserName,
                Password = new string('*', Math.Min(3, record.Password?.Length ?? 0)), // Avoid to log password from UI
                record.Passive,
                record.InstancesDefault,
                record.PrototypesDefault,
                record.SSL,
                ReasonForChange = record.Reason,
            };
            return AuditAsync(identity, action, information);
        }

        private Task AuditAsync(IDefaultIdentity identity, string action, long id, string reason)
        {
            var information = new
            {
                Id = id,
                ReasonForChange = reason
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
