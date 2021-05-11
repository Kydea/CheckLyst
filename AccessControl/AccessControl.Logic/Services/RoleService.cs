using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CheckListManager.AccessControl.Data;
using CheckListManager.AccessControl.Data.Models;
using CheckListManager.AccessControl.Logic.Authentication;
using CheckListManager.AccessControl.Logic.Exceptions;
using CheckListManager.AccessControl.Logic.Messages.Roles;
using CheckListManager.AccessControl.Logic.Messages.Users;
using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.Cache;
using CheckListManager.Common.Logic.DataEngine;
using CheckListManager.Common.Logic.Email;
using CheckListManager.Common.Logic.Queue;
using CheckListManager.Common.Logic.Services;
using CheckListManager.Common.Logic.Trail;
using Newtonsoft.Json;
using NLog;

namespace CheckListManager.AccessControl.Logic.Services
{
    public class RoleService : IRoleService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IUserRepository _userRepository;
        private readonly IQueueService _queueService;
        private readonly ISettingService _settingService;

        public RoleService(IUserRepository userRepository, IQueueService queueService, ISettingService settingService)
        {
            _userRepository = userRepository;
            _queueService = queueService;
            _settingService = settingService;
        }

        public async Task<PagingResponse<List<RolesResponse>>> GetRolesAsync(long companyId, PagingRequest<RolesRequest> request)
        {
            var take = request.ItemsPerPage;
            var roles = await _userRepository.GetRolesAsync(companyId, request.Data.Code, request.Skip, take);
            var count = await _userRepository.CountRolesAsync(companyId, request.Data.Code);

            var response = roles.Select(r => new RolesResponse
            {
                Code = r.Code,
                FunctionGroup = r.FunctionGroup,
                CreatedBy = r.CreatedBy,
                CreatedOn = r.CreatedOn,
                ModifiedBy = r.ModifiedBy,
                ModifiedOn = r.ModifiedOn
            }).ToList();

            return new PagingResponse<List<RolesResponse>>
            {
                Data = response,
                ItemsPerPage = request.ItemsPerPage,
                Count = count,
                CurrentPage = request.CurrentPage
            };
        }

        private async Task<RoleResponse> ValidateAsync(IDefaultIdentity identity, RoleRequest record)
        {
            var requireReason = await _settingService.RequireUserReasonAsync(identity.CompanyId);

            var response = new RoleResponse();
            if (string.IsNullOrWhiteSpace(record.Code))
                response.Fields["Code"] = new Field
                {
                    ValidationMessages = new List<ValidationMessage>
                    {
                        new ValidationMessage
                        {
                            Level = ValidationMessage.LevelError,
                            Message = "Invalid code"
                        }
                    }
                };

            if (requireReason && string.IsNullOrEmpty(record.Reason))
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

        public async Task<RoleResponse> LoadRoleAsync(DefaultIdentity identity, RoleRequest record)
        {
            var response = new RoleResponse();

            if (!string.IsNullOrEmpty(record.Code))
                try
                {
                    var role = await _userRepository.GetRoleAsync(identity.CompanyId, record.Code);
                    response.FunctionGroup = role.FunctionGroup;

                    response.GeneralFunctions = role.RoleFunctions
                        .Where(f => f.FunctionGroup != "Prototypes" && f.FunctionGroup != "Instances")
                        .Select(gf => new Function { functionGroup = gf.FunctionGroup, function = gf.Function })
                        .ToArray();

                    response.PrototypeFunctions = new Function[] {
                        new Function
                        {
                            functionGroup = "Prototypes",
                            functions = role.RoleFunctions.Where(f => f.FunctionGroup == "Prototypes").Select(pf => pf.Function).ToArray(),
                        }
                    };

                    response.InstanceFunctions = new Function[] {
                        new Function
                        {
                            functionGroup = "Instances",
                            functions = role.RoleFunctions.Where(f => f.FunctionGroup == "Instances").Select(pf => pf.Function).ToArray(),
                        }
                    };
                }
                catch (Exception exception)
                {
                    response.ValidationMessages.Add(new ValidationMessage
                    {
                        Level = ValidationMessage.LevelError,
                        Message = exception.Message
                    });
                    response.Valid = false;
                }

            return response;
        }

        private void BuildFunctions(DefaultIdentity identity, RoleRequest record, Roles Role)
        {
            foreach (var function in record.GeneralFunctions)
                Role.RoleFunctions.Add(new RoleFunctions
                {
                    Company = identity.CompanyId,
                    Role = record.Code,
                    FunctionGroup = function.functionGroup,
                    Function = function.function,
                    CreatedOn = DateTimeOffset.UtcNow,
                    CreatedBy = identity.Code
                });

            foreach (var row in record.PrototypeFunctions)
                if (row.functions != null)
                    foreach (var function in row.functions)
                        Role.RoleFunctions.Add(new RoleFunctions
                        {
                            Company = identity.CompanyId,
                            Role = record.Code,
                            FunctionGroup = "Prototypes",
                            Function = function,
                            CreatedOn = DateTimeOffset.UtcNow,
                            CreatedBy = identity.Code
                        });

            foreach (var row in record.InstanceFunctions)
                if (row.functions != null)
                    foreach (var function in row.functions)
                        Role.RoleFunctions.Add(new RoleFunctions
                        {
                            Company = identity.CompanyId,
                            Role = record.Code,
                            FunctionGroup = "Instances",
                            Function = function,
                            CreatedOn = DateTimeOffset.UtcNow,
                            CreatedBy = identity.Code
                        });
        }

        private static string SerializeRoleInformation(RoleRequest role, IEnumerable<RoleFunctions> roleFunctions)
        {
            return JsonConvert.SerializeObject(new
            {
                role.Code,
                role.FunctionGroup,
                Functions = roleFunctions.Select(uf => new
                {
                    uf.FunctionGroup,
                    uf.Function
                }),
                ReasonForChange = role.Reason
            });
        }

        public async Task<RoleResponse> AddRoleAsync(DefaultIdentity identity, RoleRequest record)
        {
            var response = await ValidateAsync(identity, record);
            if (response.Valid)
                try
                {
                    var Role = new Roles
                    {
                        Code = record.Code,
                        FunctionGroup = record.FunctionGroup
                    };

                    BuildFunctions(identity, record, Role);

                    await _userRepository.AddRoleAsync(identity, Role);

                    await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
                    {
                        Company = identity.CompanyId,
                        User = identity.Code,
                        Action = TrailAction.RoleCreated,
                        Date = DateTimeOffset.UtcNow,
                        Information = SerializeRoleInformation(record, Role.RoleFunctions)
                    });
                }
                catch (Exception exception)
                {
                    response.ValidationMessages.Add(new ValidationMessage
                    {
                        Level = ValidationMessage.LevelError,
                        Message = exception.Message
                    });
                    response.Valid = false;
                }

            return response;
        }

        public async Task<RoleResponse> EditRoleAsync(DefaultIdentity identity, RoleRequest record)
        {
            var response = await ValidateAsync(identity, record);
            if (response.Valid)
                try
                {
                    var Role = new Roles
                    {
                        Code = record.Code,
                        FunctionGroup = record.FunctionGroup
                    };

                    BuildFunctions(identity, record, Role);

                    await _userRepository.EditRoleAsync(identity, Role);

                    await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
                    {
                        Company = identity.CompanyId,
                        User = identity.Code,
                        Action = TrailAction.RoleEdited,
                        Date = DateTimeOffset.UtcNow,
                        Information = SerializeRoleInformation(record, Role.RoleFunctions)
                    });
                }
                catch (Exception exception)
                {
                    response.ValidationMessages.Add(new ValidationMessage
                    {
                        Level = ValidationMessage.LevelError,
                        Message = exception.Message
                    });
                    response.Valid = false;
                }

            return response;
        }

        public async Task<RoleResponse> DeleteRoleAsync(DefaultIdentity identity, RoleRequest record)
        {
            var response = new RoleResponse();
            try
            {
                await _userRepository.DeleteRoleAsync(identity.CompanyId, record.Code);
                await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
                {
                    Company = identity.CompanyId,
                    User = identity.Code,
                    Action = TrailAction.RoleDeleted,
                    Date = DateTimeOffset.UtcNow,
                    Information = JsonConvert.SerializeObject(new
                    {
                        record.Code,
                        ReasonForChange = record.Reason
                    })
                });
            }
            catch (Exception exception)
            {
                response.ValidationMessages.Add(new ValidationMessage
                {
                    Level = ValidationMessage.LevelError,
                    Message = exception.Message
                });
                response.Valid = false;
            }

            return response;
        }
    }
}