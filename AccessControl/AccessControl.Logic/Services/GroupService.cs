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

        public async Task<List<ExportRow>> GetFunctionsAsync(long companyId, GroupsRequest request) =>
            await _userRepository.GetGroupsForExportAsync(companyId, request.Code, request.Description, request.User, request.Role);

        public async Task<PagingResponse<List<GroupsResponse>>> GetGroupsAsync(long companyId, PagingRequest<GroupsRequest> request)
        {
            var take = request.ItemsPerPage;
            var groups = await _userRepository.GetGroupsAsync(companyId, request.Data.Code, request.Data.Description, request.Data.User, request.Data.Role, request.Skip, take);
            var count = await _userRepository.CountGroupsAsync(companyId, request.Data.Code, request.Data.Description, request.Data.User, request.Data.Role);

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

            if (string.IsNullOrWhiteSpace(record.Description))
                response.Fields["Description"] = new Field
                {
                    ValidationMessages = new List<ValidationMessage>
                    {
                        new ValidationMessage
                        {
                            Level = ValidationMessage.LevelError,
                            Message = "Invalid description"
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

                    var role = group.GroupRoles.SingleOrDefault(r => r.Roles.FunctionGroup == null);
                    if (role == null)
                        response.GeneralFunctions =
                            group.GroupFunctions.Where(f => f.FunctionGroup != "Prototypes" && f.FunctionGroup != "Instances")
                                .Select(gf => new Grant { FunctionGroup = gf.FunctionGroup, Function = gf.Function })
                                .ToArray();
                    else
                        response.GeneralFunctions = new Grant[] { new Grant { FunctionGroup = null, Role = role.Role } };

                    var functions = new List<Grant>();
                    functions = group.GroupFunctions.Where(f => f.FunctionGroup == "Prototypes")
                        .GroupBy(pf => new { pf.Department, pf.PrototypeType })
                        .Select(g => new Grant
                        {
                            Department = g.Key.Department,
                            PrototypeType = g.Key.PrototypeType,
                            FunctionGroup = "Prototypes",
                            Functions = g.Select(pf => pf.Function).ToArray()
                        })
                        .ToList();
                    functions.AddRange(group.GroupRoles.Where(r => r.Roles.FunctionGroup == "Prototypes").Select(r => new Grant
                    {
                        Department = r.Department,
                        PrototypeType = r.PrototypeType,
                        FunctionGroup = "Prototypes",
                        Role = r.Role
                    }));
                    response.PrototypeFunctions = functions.ToArray();

                    functions = group.GroupFunctions.Where(f => f.FunctionGroup == "Instances")
                        .GroupBy(pf => new { pf.Department, pf.PrototypeType })
                        .Select(g => new Grant
                        {
                            Department = g.Key.Department,
                            PrototypeType = g.Key.PrototypeType,
                            FunctionGroup = "Instances",
                            Functions = g.Select(pf => pf.Function).ToArray()
                        }).ToList();
                    functions.AddRange(group.GroupRoles.Where(r => r.Roles.FunctionGroup == "Instances").Select(r => new Grant
                    {
                        Department = r.Department,
                        PrototypeType = r.PrototypeType,
                        FunctionGroup = "Instances",
                        Role = r.Role
                    }));
                    response.InstanceFunctions = functions.ToArray();

                    response.ConflictingGroups =
                        group.GroupConflicts.Select(c => c.Group2).Union(group.GroupConflicts1.Select(c => c.Group1)).Distinct().ToList();
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
            if (record.GeneralFunctions.Length == 1 && record.GeneralFunctions[0].Role != null)
                Group.GroupRoles.Add(new GroupRoles
                {
                    Company = identity.CompanyId,
                    Role = record.GeneralFunctions[0].Role,
                    CreatedOn = DateTimeOffset.UtcNow,
                    CreatedBy = identity.Code
                });
            else
                foreach (var function in record.GeneralFunctions)
                    Group.GroupFunctions.Add(new GroupFunctions
                    {
                        Company = identity.CompanyId,
                        Group = record.Code,
                        FunctionGroup = function.FunctionGroup,
                        Function = function.Function,
                        CreatedOn = DateTimeOffset.UtcNow,
                        CreatedBy = identity.Code
                    });

            foreach (var row in record.PrototypeFunctions)
                if (row.Role != null)
                    Group.GroupRoles.Add(new GroupRoles
                    {
                        Company = identity.CompanyId,
                        Role = row.Role,
                        Department = row.Department,
                        PrototypeType = row.PrototypeType,
                        CreatedOn = DateTimeOffset.UtcNow,
                        CreatedBy = identity.Code
                    });
                else if (row.Functions != null)
                    foreach (var function in row.Functions)
                        Group.GroupFunctions.Add(new GroupFunctions
                        {
                            Company = identity.CompanyId,
                            Group = record.Code,
                            Department = row.Department,
                            PrototypeType = row.PrototypeType,
                            FunctionGroup = "Prototypes",
                            Function = function,
                            CreatedOn = DateTimeOffset.UtcNow,
                            CreatedBy = identity.Code
                        });

            foreach (var row in record.InstanceFunctions)
                if (row.Role != null)
                    Group.GroupRoles.Add(new GroupRoles
                    {
                        Company = identity.CompanyId,
                        Role = row.Role,
                        Department = row.Department,
                        PrototypeType = row.PrototypeType,
                        CreatedOn = DateTimeOffset.UtcNow,
                        CreatedBy = identity.Code
                    });
                else if (row.Functions != null)
                    foreach (var function in row.Functions)
                        Group.GroupFunctions.Add(new GroupFunctions
                        {
                            Company = identity.CompanyId,
                            Group = record.Code,
                            Department = row.Department,
                            PrototypeType = row.PrototypeType,
                            FunctionGroup = "Instances",
                            Function = function,
                            CreatedOn = DateTimeOffset.UtcNow,
                            CreatedBy = identity.Code
                        });
        }

        private void BuildConflicts(DefaultIdentity identity, GroupRequest record, Groups Group)
        {
            if (record.ConflictingGroups != null)
                foreach (var conflictingGroup in record.ConflictingGroups)
                    Group.GroupConflicts.Add(new GroupConflicts
                    {
                        Group2 = conflictingGroup
                    });
        }

        private static string SerializeGroupInformation(GroupRequest group, IEnumerable<GroupFunctions> groupFunctions, IEnumerable<GroupRoles> groupRoles)
        {
            return JsonConvert.SerializeObject(new
            {
                group.Code,
                group.Description,
                Functions = groupFunctions.Select(uf => new
                {
                    Department = uf.Department.HasValue ? uf.Department + " " + uf.Departments?.Name : null,
                    PrototypeType = uf.PrototypeType.HasValue ? uf.PrototypeType + " " + uf.PrototypeTypes?.Name : null,
                    uf.FunctionGroup,
                    uf.Function
                }),
                Roles = groupRoles.Select(gr => new
                {
                    Department = gr.Department.HasValue ? gr.Department + " " + gr.Departments?.Name : null,
                    PrototypeType = gr.PrototypeType.HasValue ? gr.PrototypeType + " " + gr.PrototypeTypes?.Name : null,
                    gr.Role
                }),
                group.ConflictingGroups,
                ReasonForChange = group.Reason
            });
        }

        private async Task TraceConflicts(DefaultIdentity identity, IEnumerable<string> conflicts, string reason)
        {
            foreach (var conflict in conflicts)
            {
                var group = await _userRepository.GetGroupAsync(identity.CompanyId, conflict);
                await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
                {
                    Company = identity.CompanyId,
                    User = identity.Code,
                    Action = TrailAction.GroupEdited,
                    Date = DateTimeOffset.UtcNow,
                    Information = SerializeGroupInformation(new GroupRequest
                    {
                        Code = group.Code,
                        Description = group.Description,
                        ConflictingGroups = group.GroupConflicts.Select(gc => gc.Group2).Union(group.GroupConflicts1.Select(gc => gc.Group1)).Distinct().ToList(),
                        Reason = reason
                    }, group.GroupFunctions, group.GroupRoles)
                });
            }
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
                    BuildConflicts(identity, record, Group);
                    await _userRepository.SaveChangesAsync();

                    await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
                    {
                        Company = identity.CompanyId,
                        User = identity.Code,
                        Action = TrailAction.GroupCreated,
                        Date = DateTimeOffset.UtcNow,
                        Information = SerializeGroupInformation(record, Group.GroupFunctions, Group.GroupRoles)
                    });

                    await TraceConflicts(identity, Group.GroupConflicts.Select(c => c.Group2).ToList(), record.Reason);
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
                    BuildConflicts(identity, record, Group);
                    var ExistingConflicts = new List<string>();
                    var ExistingGroup = await _userRepository.EditGroupAsync(identity, Group, ExistingConflicts);

                    await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
                    {
                        Company = identity.CompanyId,
                        User = identity.Code,
                        Action = TrailAction.GroupEdited,
                        Date = DateTimeOffset.UtcNow,
                        Information = SerializeGroupInformation(record, Group.GroupFunctions, Group.GroupRoles)
                    });

                    var CurrentConflicts = new HashSet<string>(ExistingGroup.GroupConflicts.Select(c => c.Group2).ToList());
                    var ChangedConflicts = new HashSet<string>(ExistingConflicts).Except(CurrentConflicts).Union(CurrentConflicts.Except(ExistingConflicts));

                    await TraceConflicts(identity, ChangedConflicts, record.Reason);
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

        public async Task<List<string>> GetSyntheticDataAsync(long companyId) =>
            (await _userRepository.GetGroupsAsync(companyId, string.Empty, string.Empty, string.Empty, string.Empty, 0, 10000)).Select(g => g.Code).ToList();
    }
}