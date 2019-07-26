using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.DataEngine;
using CheckListManager.Common.Logic.Terminal.FTP;
using CheckListManager.Common.Logic.Trail;
using CheckListManager.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Infrastructure.Logic.FTPServers
{
    public class FTPServersService : IFTPServersService
    {
        private readonly IInfrastructureRepository _infrastructureRepository;
        private readonly ICommonRepository _commonRepository;

        public FTPServersService(IInfrastructureRepository infrastructureRepository, ICommonRepository commonRepository)
        {
            _infrastructureRepository = infrastructureRepository;
            _commonRepository = commonRepository;
        }

        public async Task<PagingResponse<List<FTPServersRow>>> GetDataAsync(IDefaultIdentity identity, PagingRequest<FTPServersRequest> Form)
            => await _infrastructureRepository
                .GetFTPServersAsync(
                    identity,
                    d => new FTPServersRow
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
                    },
                    Form.Data.Name, Form.Data.Host, Form.CurrentPage, Form.ItemsPerPage
                );

        private FTPServersResponse Validate(FTPServersRow record, bool IsNew)
        {
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
            return response;
        }

        public async Task<FTPServersResponse> AddOrEdit(IDefaultIdentity identity, FTPServersRow record, bool isNew)
        {
            var response = Validate(record, isNew);
            if (response.Valid)
                try
                {
                    var FTPServer = new Common.Data.FTPServers
                    {
                        Name = record.Name,
                        Host = record.Host,
                        Port = record.Port ?? 21,
                        Folder = record.Folder,
                        UserName = record.UserName,
                        Password = record.Password,
                        Passive = record.Passive,
                        SSL = record.SSL,
                        PrototypesDefault = record.PrototypesDefault,
                        InstancesDefault = record.InstancesDefault
                    };
                    if (isNew)
                        await _infrastructureRepository.AddFTPServerAsync(identity, FTPServer);
                    else
                    {
                        FTPServer.ID = record.ID.Value;
                        await _infrastructureRepository.EditFTPServerAsync(identity, FTPServer);
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

        public async Task<FTPServersResponse> Delete(IDefaultIdentity identity, long id)
        {
            var response = new FTPServersResponse();
            try
            {
                await _infrastructureRepository.DeleteFTPServerAsync(identity, id);
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
    }
}
