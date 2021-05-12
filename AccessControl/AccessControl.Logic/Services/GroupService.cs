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
using CheckListManager.AccessControl.Logic.Messages.Groups;
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
    public class GroupService : IGroupService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IUserRepository _userRepository;
        private readonly IQueueService _queueService;
        private readonly ISettingService _settingService;

        public GroupService(IUserRepository userRepository, IQueueService queueService, ISettingService settingService)
        {
            _userRepository = userRepository;
            _queueService = queueService;
            _settingService = settingService;
        }

        public async Task<PagingResponse<List<GroupsResponse>>> GetGroupsAsync(long companyId, PagingRequest<GroupsRequest> request)
        {
            var take = request.ItemsPerPage;
            var groups = await _userRepository.GetGroupsAsync(companyId, request.Data.Code, request.Data.Description, request.Skip, take);
            var count = await _userRepository.CountGroupsAsync(companyId, request.Data.Code, request.Data.Description);

            var response = groups.Select(g => new GroupsResponse
            {
                Code = g.Code,
                Description = g.Description,
                CreatedBy = g.CreatedBy,
                CreatedOn = g.CreatedOn,
                ModifiedBy = g.ModifiedBy,
                ModifiedOn = g.ModifiedOn
            }).ToList();

            return new PagingResponse<List<GroupsResponse>>
            {
                Data = response,
                ItemsPerPage = request.ItemsPerPage,
                Count = count,
                CurrentPage = request.CurrentPage
            };
        }

        private async Task<GroupResponse> ValidateAsync(IDefaultIdentity identity, GroupRequest record)
        {
            var requireReason = await _settingService.RequireUserReasonAsync(identity.CompanyId);

            var response = new GroupResponse();
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

        public async Task<GroupResponse> LoadGroupAsync(DefaultIdentity identity, GroupRequest record)
        {
            var response = new GroupResponse();

            if (!string.IsNullOrEmpty(record.Code))
                try
                {
                    var group = await _userRepository.GetGroupAsync(identity.CompanyId, record.Code);
                    response.Description = group.Description;

                    response.GeneralFunctions = group.GroupFunctions.Where(f =>
                            f.FunctionGroup != "Prototypes" && f.FunctionGroup != "Instances")
                        .Select(gf => new Function { functionGroup = gf.FunctionGroup, function = gf.Function })
                        .ToArray();

                    response.PrototypeFunctions = group.GroupFunctions.Where(f => f.FunctionGroup == "Prototypes")
                        .GroupBy(pf => new { pf.Department, pf.PrototypeType })
                        .Select(g => new Function
                        {
                            department = g.Key.Department,
                            prototypeType = g.Key.PrototypeType,
                            functionGroup = "Prototypes",
                            functions = g.Select(pf => pf.Function).ToArray(),
                        })
                        .ToArray();

                    response.InstanceFunctions = group.GroupFunctions.Where(f => f.FunctionGroup == "Instances")
                        .GroupBy(pf => new { pf.Department, pf.PrototypeType })
                        .Select(g => new Function
                        {
                            department = g.Key.Department,
                            prototypeType = g.Key.PrototypeType,
                            functionGroup = "Instances",
                            functions = g.Select(pf => pf.Function).ToArray(),
                        })
                        .ToArray();
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

        private void BuildFunctions(DefaultIdentity identity, GroupRequest record, Groups Group)
        {
            foreach (var function in record.GeneralFunctions)
                Group.GroupFunctions.Add(new GroupFunctions
                {
                    Company = identity.CompanyId,
                    Group = record.Code,
                    FunctionGroup = function.functionGroup,
                    Function = function.function,
                    CreatedOn = DateTimeOffset.UtcNow,
                    CreatedBy = identity.Code
                });

            foreach (var row in record.PrototypeFunctions)
                if (row.functions != null)
                    foreach (var function in row.functions)
                        Group.GroupFunctions.Add(new GroupFunctions
                        {
                            Company = identity.CompanyId,
                            Group = record.Code,
                            Department = row.department,
                            PrototypeType = row.prototypeType,
                            FunctionGroup = "Prototypes",
                            Function = function,
                            CreatedOn = DateTimeOffset.UtcNow,
                            CreatedBy = identity.Code
                        });

            foreach (var row in record.InstanceFunctions)
                if (row.functions != null)
                    foreach (var function in row.functions)
                        Group.GroupFunctions.Add(new GroupFunctions
                        {
                            Company = identity.CompanyId,
                            Group = record.Code,
                            Department = row.department,
                            PrototypeType = row.prototypeType,
                            FunctionGroup = "Instances",
                            Function = function,
                            CreatedOn = DateTimeOffset.UtcNow,
                            CreatedBy = identity.Code
                        });
        }

        private static string SerializeGroupInformation(GroupRequest group, IEnumerable<GroupFunctions> groupFunctions)
        {
            return JsonConvert.SerializeObject(new
            {
                group.Code,
                group.Description,
                Functions = groupFunctions.Select(uf => new
                {
                    uf.FunctionGroup,
                    uf.Function
                }),
                ReasonForChange = group.Reason
            });
        }

        public async Task<GroupResponse> AddGroupAsync(DefaultIdentity identity, GroupRequest record)
        {
            var response = await ValidateAsync(identity, record);
            if (response.Valid)
                try
                {
                    var Group = new Groups
                    {
                        Code = record.Code,
                        Description = record.Description
                    };

                    BuildFunctions(identity, record, Group);

                    await _userRepository.AddGroupAsync(identity, Group);

                    await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
                    {
                        Company = identity.CompanyId,
                        User = identity.Code,
                        Action = TrailAction.GroupCreated,
                        Date = DateTimeOffset.UtcNow,
                        Information = SerializeGroupInformation(record, Group.GroupFunctions)
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

        public async Task<GroupResponse> EditGroupAsync(DefaultIdentity identity, GroupRequest record)
        {
            var response = await ValidateAsync(identity, record);
            if (response.Valid)
                try
                {
                    var Group = new Groups
                    {
                        Code = record.Code,
                        Description = record.Description
                    };

                    BuildFunctions(identity, record, Group);

                    await _userRepository.EditGroupAsync(identity, Group);

                    await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
                    {
                        Company = identity.CompanyId,
                        User = identity.Code,
                        Action = TrailAction.GroupEdited,
                        Date = DateTimeOffset.UtcNow,
                        Information = SerializeGroupInformation(record, Group.GroupFunctions)
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

        public async Task<GroupResponse> DeleteGroupAsync(DefaultIdentity identity, GroupRequest record)
        {
            var response = new GroupResponse();
            try
            {
                await _userRepository.DeleteGroupAsync(identity.CompanyId, record.Code);
                await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
                {
                    Company = identity.CompanyId,
                    User = identity.Code,
                    Action = TrailAction.GroupDeleted,
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