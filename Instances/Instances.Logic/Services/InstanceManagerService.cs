using System;
using System.Linq;
using System.Net;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Core.Network;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.Cache;
using CheckListManager.Common.Logic.Json;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Services;
using CheckListManager.Instances.Data;
using CheckListManager.Instances.Logic.Exceptions;
using CheckListManager.Instances.Logic.Messages;
using CheckListManager.Instances.Logic.Models;
using CheckListManager.Instances.Logic.Models.CheckLists;
using Newtonsoft.Json;
using NLog;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Authentication;
using CheckListManager.Common.Logic.WorkFlows;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using CheckListManager.Common.Logic.Queue;
using CheckListManager.Common.Logic.Trail;
using CheckListManager.Common.Logic.FTP;
using CheckListManager.Common.Logic.DataSources;
using CheckListManager.Common.Logic;
using CheckListManager.Common.Logic.Api.Instances;
using MultipartDataMediaFormatter.Infrastructure;
using CheckListManager.Common.Logic.Email;
using CheckListManager.Common.Logic.Models;
using CheckListManager.Instances.Logic.ImageUtilities;
using CheckListManager.Common.Logic.Dependency;

namespace CheckListManager.Instances.Logic.Services
{
    public class InstanceManagerService : IInstanceManagerService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IServiceClient _client;
        private readonly IInstancesRepository _instancesRepository;
        private readonly JsonConverter[] _converters;
        private readonly IInstanceCacheService _instanceCache;
        private readonly IConfigurationService _configurationService;
        private readonly ISettingService _settingService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IWorkFlowsService _workFlowService;
        private readonly IQueueService _queueService;
        private readonly ICommonRepository _commonRepository;
        private readonly IRedisCache _redisCache;
        private readonly IStorageService _storageService;

        public InstanceManagerService(
            IServiceClient client,
            IInstancesRepository instancesRepository,
            CustomJsonConverter[] converters,
            IInstanceCacheService instanceCache,
            IConfigurationService configurationService,
            ISettingService settingService,
            IAuthenticationService authenticationService,
            IWorkFlowsService workFlowService,
            IQueueService queueService,
            ICommonRepository commonRepository,
            IRedisCache redisCache,
            IStorageService storageService
        )
        {
            _client = client;
            _instancesRepository = instancesRepository;
            _converters = converters.ToArray<JsonConverter>();
            _instanceCache = instanceCache;
            _configurationService = configurationService;
            _settingService = settingService;
            _authenticationService = authenticationService;
            _workFlowService = workFlowService;
            _queueService = queueService;
            _commonRepository = commonRepository;
            _redisCache = redisCache;
            _storageService = storageService;
        }

        #region cache object task

        public async Task<InstanceCacheObject> GetCacheObjectForDownloadAsync(long company, string code, bool includeData)
        {
            var cacheObject = await GetCacheObjectAsync(company, code, includeData);
            cacheObject.Instance.UpdateReadOnly(null, cacheObject.InstanceState);
            return cacheObject;
        }

        public async Task<InstanceCacheObject> GetCacheObjectForEditAsync(IDefaultIdentity identity, string code, bool includeData)
        {
            var cacheObject = await GetCacheObjectAsync(identity.CompanyId, code, includeData);
            if (includeData)
            {
                cacheObject.Instance.ReadOnly =
                    !cacheObject.InstanceState.In(InstanceState.Draft, InstanceState.Progress)
                    || !HasPermission(identity, "Edit", cacheObject.VersionState, cacheObject.Department, cacheObject.PrototypeType)
                    || cacheObject.Year == 0 && !cacheObject.VersionState.In(VersionState.Draft, VersionState.InSimulation);
                cacheObject.Instance.UpdateReadOnly(identity.Code, cacheObject.InstanceState);
            }
            return cacheObject;
        }

        private async Task<InstanceCacheObject> GetCacheObjectAsync(long company, string code, bool includeData)
        {
            var cacheObject = await _instanceCache.GetAsync(code, includeData);
            if (cacheObject == null)
            {
                var instance = await _instancesRepository.GetAsync(company, code);
                if (instance == null)
                    throw new InstanceException("Instance not found");

                cacheObject = await CreateInstanceFromDatabase(company, instance);
                if (cacheObject.InstanceState == InstanceState.Progress)
                    cacheObject.NeedToCache = true;
            }

            if (includeData && !cacheObject.Instance.CheckVersion(2, false)) //version 2 introduces persistency
                cacheObject.Instance.UpdateAll(cacheObject);

            return cacheObject;
        }

        public async Task SaveCacheObjectAsync(InstanceCacheObject cacheObject)
        {
            if (!cacheObject.Instance.CheckVersion(null, true) || cacheObject.NeedToCache)
                await _instanceCache.SetAsync(cacheObject);
            else
                await _instanceCache.TouchAsync(cacheObject.Code);
        }

        #endregion

        #region record tasks

        public async Task<InstanceCacheObject> CreateInstanceAsync(IDefaultIdentity identity, string userCode, long prototype, int? version, Dictionary<string, string> parameters)
        {
            try
            {
                if (!version.HasValue)
                    version = await _instancesRepository.GetActivePrototypeVersion(prototype);
                if (!version.HasValue)
                    throw new InstanceException($"The prototype {prototype} has no active version");

                var source = await GetPrototype(prototype, version.Value);
                if (!source.State.In(VersionState.Draft, VersionState.InSimulation, VersionState.Active))
                    throw new InstanceException($"Invalid state of the prototype version {prototype}.{version}: {source.State}"); //double check

                if (!HasPermission(identity, "Create", source.State, source.Department, source.PrototypeType))
                    throw new InstanceException("Access denied");

                var cacheObject = new InstanceCacheObject
                {
                    CompanyId = identity.CompanyId,
                    Instance = source.CheckList,
                    Code = $"{prototype:X}-{version:X}-{Guid.NewGuid()}",
                    Department = source.Department,
                    DepartmentName = source.DepartmentName,
                    PrototypeType = source.PrototypeType,
                    PrototypeTypeName = source.PrototypeTypeName,
                    Prototype = prototype,
                    PrototypeName = source.Name,
                    PrototypeDescription = source.Description,
                    Version = version.Value,
                    VersionState = source.State,
                    InstanceState = InstanceState.Draft,
                    CreatedBy = userCode,
                    CreatedOn = DateTimeOffset.UtcNow,
                    NeedToCache = true
                };

                cacheObject.Instance.Setup(cacheObject, await _settingService.DecimalSeparatorAsync(cacheObject.CompanyId), await _settingService.TimeZoneAsync(cacheObject.CompanyId), true);
                if (parameters != null)
                    foreach (var parameter in parameters)
                        if (cacheObject.Instance.Elements.TryGetValue(parameter.Key, out ICheckListElement element) && element is ICheckListItem item)
                            await HandleValueChangeAsync(cacheObject, userCode, item.Id, parameter.Value);

                cacheObject.Instance.UpdateAll(cacheObject);
                cacheObject.Instance.UpdateReadOnly(identity.Code, cacheObject.InstanceState);

                return cacheObject;
            }
            catch (InstanceException)
            {
                throw;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Unable to create the instance");
                throw new InstanceException("Unable to create the instance", e);
            }
        }

        public async Task<InstanceCacheObject> OpenInstanceAsync(string code, IDefaultIdentity identity)
        {
            try
            {
                var cacheObject = await GetCacheObjectForEditAsync(identity, code, true);
                if (cacheObject.Id.HasValue)
                    throw new InstanceException("Instance already saved");
                if (cacheObject.InstanceState != InstanceState.Draft)
                    throw new InstanceException("Instance state not valid");
                if (!HasPermission(identity, "Create", cacheObject.VersionState, cacheObject.Department, cacheObject.PrototypeType))
                    throw new InstanceException("Access denied");

                if (!cacheObject.Instance.Header.Valid)
                    throw new InstanceException("Header not valid");

                cacheObject.InstanceState = InstanceState.Progress;

                var row = new Common.Data.Instances
                {
                    Company = identity.CompanyId,
                    Data = JsonConvert.SerializeObject(cacheObject.Instance, _converters),
                    Code = code,
                    Description = cacheObject.Instance.Description,
                    InstanceState = cacheObject.InstanceState,
                    Prototype = cacheObject.Prototype,
                    PrototypeName = cacheObject.PrototypeName,
                    Version = cacheObject.Version,
                    Year = cacheObject.VersionState == VersionState.Active ? DateTimeOffset.UtcNow.Year : 0,
                    CreatedBy = cacheObject.CreatedBy,
                    CreatedOn = cacheObject.CreatedOn,
                    ModifiedBy = identity.Code,
                    ModifiedOn = DateTimeOffset.UtcNow
                };

                try
                {
                    row = await _instancesRepository.BeginCreateInstanceAsync(row);

                    cacheObject.Id = row.ID;
                    cacheObject.Year = row.Year;
                    cacheObject.Number = row.Number;
                    if (cacheObject.Instance.Pages.Any()) ((Page)cacheObject.Instance.Pages[0]).Lock = new Lock(identity.Code);
                    TrackChanges(cacheObject, identity.Code);

                    cacheObject.Instance.UpdateInstanceRuntimeValues(cacheObject);
                    cacheObject.Instance.Update(DependencyRelation.Properties.Formula);
                    cacheObject.Instance.UpdateReadOnly(identity.Code, InstanceState.Progress);

                    _instancesRepository.SaveChildren(row, CreateTags(cacheObject), CreateExpressionValues(cacheObject));

                    await _instancesRepository.SaveChangesAsync();

                    try
                    {
                        await _instancesRepository.ValidateUniqueExpressions(row.ID);
                    }
                    catch (AggregateException Exception)
                    {
                        throw new InstanceException(string.Join(" - ", Exception.InnerExceptions.Select(e => e.Message)));
                    }

                    _instancesRepository.EndCreateInstanceAsync(row);
                }
                catch
                {
                    _instancesRepository.CancelCreateInstanceAsync();
                    throw;
                }

                await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
                {
                    Company = identity.CompanyId,
                    User = identity.Code,
                    Action = TrailAction.InstanceCreated,
                    Date = DateTimeOffset.UtcNow,
                    CheckList = cacheObject.InstanceCode
                });

                return cacheObject;
            }
            catch (InstanceException)
            {
                throw;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Unable to open the instance");
                throw new InstanceException("Unable to open the instance", e);
            }
        }

        public async Task SaveInstanceAsync(string code, IDefaultIdentity identity)
        {
            try
            {
                var cacheObject = await GetCacheObjectForEditAsync(identity, code, false);
                if (cacheObject.InstanceState != InstanceState.Progress)
                    throw new InstanceException("Instance state not valid");
                if (!HasPermission(identity, "Edit", cacheObject.VersionState, cacheObject.Department, cacheObject.PrototypeType))
                    throw new InstanceException("Access denied");

                await _instanceCache.TouchAsync(cacheObject.Code);
                await SaveAsync(cacheObject, identity, false, false, false, cacheObject.ModifiedBy, cacheObject.ModifiedOn);
            }
            catch (InstanceException)
            {
                throw;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Unable to save the instance");
                throw new InstanceException("Unable to save the instance", e);
            }
        }

        public async Task<InstanceCacheObject> CompleteInstanceAsync(string code, IDefaultIdentity identity)
        {
            try
            {
                var cacheObject = await GetCacheObjectForEditAsync(identity, code, true);
                if (cacheObject.InstanceState != InstanceState.Progress)
                    throw new InstanceException("Invalid instance state");
                if (!HasPermission(identity, "Edit", cacheObject.VersionState, cacheObject.Department, cacheObject.PrototypeType))
                    throw new InstanceException("Access denied");

                var InvalidItems = cacheObject.Instance.Items.Values.Where(i => !i.Removed && !i.Valid && !i.IteratorModelItem);
                if (InvalidItems.Any())
                    throw new InstanceException($"The following items are not valid: {string.Join(", ", InvalidItems.Select(i => i.Id.ToString()))}");

                var InvalidPage = cacheObject.Instance.Pages.FirstOrDefault(p => !p.Valid);
                if (InvalidPage != null)
                    throw new InstanceException($"The page {InvalidPage.Description} is not valid{(InvalidPage.ValidityMessage == null ? string.Empty : $": {InvalidPage.ValidityMessage}")}");

                cacheObject = await SaveAsync(cacheObject, identity, true, true, false, identity.Code, DateTimeOffset.UtcNow);
                cacheObject.Instance.UpdateReadOnly(identity.Code, InstanceState.Completed);

                await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
                {
                    Company = identity.CompanyId,
                    User = identity.Code,
                    Action = TrailAction.InstanceStateChanged,
                    Date = DateTimeOffset.UtcNow,
                    CheckList = cacheObject.InstanceCode,
                    Information = JsonConvert.SerializeObject(new { State = cacheObject.InstanceState })
                });

                return cacheObject;
            }
            catch (InstanceException)
            {
                throw;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Unable to save the instance");
                throw new InstanceException("Unable to save the instance", e);
            }
        }

        public async Task DeleteInstanceAsync(string code, IDefaultIdentity identity, string note)
        {
            try
            {
                var cacheObject = await GetCacheObjectForEditAsync(identity, code, false);
                if (cacheObject == null)
                    throw new InstanceException("Instance state not valid");
                if (!(cacheObject.VersionState == VersionState.Draft || cacheObject.VersionState == VersionState.InSimulation) && cacheObject.InstanceState == InstanceState.Progress && string.IsNullOrWhiteSpace(note))
                    throw new InstanceException("Provide a note for the deletion");
                if (
                    !(cacheObject.InstanceState == InstanceState.Draft && HasPermission(identity, "Edit", cacheObject.VersionState, cacheObject.Department, cacheObject.PrototypeType))
                    && !HasPermission(identity, "Delete", cacheObject.VersionState, cacheObject.Department, cacheObject.PrototypeType)
                )
                    throw new InstanceException("Access denied");

                await DeleteInstanceAsync(cacheObject, identity, note);

                await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
                {
                    Company = identity.CompanyId,
                    User = identity.Code,
                    Action = TrailAction.InstanceStateChanged,
                    Date = DateTimeOffset.UtcNow,
                    CheckList = cacheObject.InstanceCode,
                    Information = JsonConvert.SerializeObject(new { State = InstanceState.Deleted, Note = note })
                });

            }
            catch (InstanceException)
            {
                throw;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Unable to delete the instance");
                throw new InstanceException("Unable to delete the instance", e);
            }
        }

        private void AddTags(Dictionary<string, InstanceTags> Tags, long InstanceID, ICheckListGroup Group)
        {
            if (Group != null)
                foreach (var Item in Group.Items)
                    if (Item is ICheckListGroup)
                        AddTags(Tags, InstanceID, (ICheckListGroup)Item);
                    else if (!string.IsNullOrWhiteSpace(Item.Description) && (Item.TypeName == "Field" || Item.TypeName == "Signature") && !Tags.ContainsKey(Item.Description))
                        Tags[Item.Description] = new InstanceTags { Instance = InstanceID, Tag = Item.Description, Value = (Item.TypeName == "Field" ? ((Field)Item).Value : ((ICheckListSignature)Item).SignedBy) ?? string.Empty };
        }

        private IEnumerable<InstanceTags> CreateTags(InstanceCacheObject cacheObject)
        {
            var Tags = new Dictionary<string, InstanceTags>();
            AddTags(Tags, cacheObject.Id.Value, cacheObject.Instance?.Header);
            return Tags.Values;
        }

        private IEnumerable<InstanceExpressionValues> CreateExpressionValues(InstanceCacheObject cacheObject)
        {
            return cacheObject.Instance.Expressions.Where(Expression => Expression.Value.Exception && Expression.Value.Value.BooleanValue || Expression.Value.Unique && !Expression.Value.Value.IsNull).Select(e => new InstanceExpressionValues
            {
                Instance = cacheObject.Id.Value,
                Code = e.Key,
                Description = e.Value.Description,
                Value = e.Value.Value.SerializedValue,
                Exception = e.Value.Exception,
                Unique = e.Value.Unique
            });
        }

        private async Task<InstanceCacheObject> SaveAsync(InstanceCacheObject cacheObject, IDefaultIdentity identity, bool close, bool complete, bool alreadyLocked, string ModifiedBy, DateTimeOffset? ModifiedOn)
        {
            LockObject lockObject = null;

            try
            {
                if (!alreadyLocked)
                {
                    lockObject = await _instanceCache.LockAsync(cacheObject.Code);
                    if (!lockObject.IsAcquired)
                        throw new InstanceException("Instance is currently being updated by another user");
                    cacheObject = await _instanceCache.GetAsync(cacheObject.Code, true);
                }

                var row = await _instancesRepository.GetAsync(cacheObject.CompanyId, cacheObject.Code);
                if (row == null)
                {
                    await _instanceCache.RemoveAsync(cacheObject.Code);
                    throw new InstanceException("Instance not found");
                }

                if (!cacheObject.Instance.CheckVersion(2, true)) //version 2 introduces persistency
                    cacheObject.Instance.UpdateAll(cacheObject);

                row.Data = JsonConvert.SerializeObject(cacheObject.Instance, _converters);
                row.Description = cacheObject.Instance.Description;
                row.FileName =
                    !cacheObject.Instance.FTPExport ? null :
                    !string.IsNullOrWhiteSpace(cacheObject.Instance.FileName) ? cacheObject.Instance.FileName :
                    $"Instance{cacheObject.InstanceCode}";
                row.ModifiedOn = ModifiedOn;
                row.ModifiedBy = ModifiedBy;

                _instancesRepository.SaveChildren(row, CreateTags(cacheObject), CreateExpressionValues(cacheObject));

                if (complete)
                {
                    long? WorkFlowApplicationId = null;
                    if (!row.PrototypeVersions.VersionState.In(VersionState.Draft, VersionState.InSimulation) && row.PrototypeVersions.InstanceConfirmationWorkFlow.HasValue)
                    {
                        var WorkFlowApplication = await _workFlowService.CreateWorkFlowApplicationAsync(identity, new CreateRequest
                        {
                            WorkFlow = row.PrototypeVersions.InstanceConfirmationWorkFlow.Value,
                            Instance = row.ID,
                            Action = "Instance – state to Confirmed"
                        });
                        WorkFlowApplicationId = WorkFlowApplication.ID;
                        cacheObject.InstanceState = row.InstanceState = InstanceState.Completed;

                        row.InstancesHistory.Add(new InstancesHistory
                        {
                            Date = ModifiedOn ?? DateTimeOffset.UtcNow,
                            User = ModifiedBy,
                            State = InstanceState.Completed
                        });
                    }
                    else
                    {
                        if (!HasPermission(identity, "Confirm", cacheObject.VersionState, cacheObject.Department, cacheObject.PrototypeType))
                            throw new InstanceException("There is not a confirmation workflow for this instance, the function Confirm is needed to complete.");
                        cacheObject.InstanceState = row.InstanceState = InstanceState.Confirmed;

                        if (!string.IsNullOrEmpty(row.FileName))
                            await _queueService.SendObjectAsync(QueueName.InstancesFTP, new InstancesFTPMessage
                            {
                                Company = identity.CompanyId,
                                ID = row.ID,
                                Path = row.FileName,
                                Code = cacheObject.InstanceCode
                            });
                    }

                    row.InstancesHistory.Add(new InstancesHistory
                    {
                        Date = ModifiedOn ?? DateTimeOffset.UtcNow,
                        User = ModifiedBy,
                        State = row.InstanceState,
                        WorkFlowApplication = WorkFlowApplicationId
                    });
                }
                if (close)
                    await Task.WhenAll(
                        _instancesRepository.SaveChangesAsync(),
                        _instanceCache.RemoveAsync(cacheObject.Code)
                    );
                else
                    await Task.WhenAll(
                        _instancesRepository.SaveChangesAsync(),
                        _instanceCache.TouchAsync(cacheObject.Code)
                    );
                return cacheObject;
            }
            finally
            {
                lockObject?.Dispose();
            }
        }

        private async Task DeleteInstanceAsync(InstanceCacheObject cacheObject, IDefaultIdentity identity, string note)
        {
            if (cacheObject.InstanceState == InstanceState.Progress)
            {
                var row = await _instancesRepository.GetAsync(cacheObject.CompanyId, cacheObject.Code);
                if (row == null)
                    throw new InstanceException("Instance not found");

                if (cacheObject.VersionState == VersionState.Draft || cacheObject.VersionState == VersionState.InSimulation)
                {
                    _instancesRepository.DeleteInstance(row);
                    await _storageService.DeleteRecursiveAsync(
                        _configurationService.InstancesStorageBucket,
                        GetAttachmentKeyPrefix(identity, cacheObject.Code)
                    );
                }
                else
                {
                    row.ModifiedOn = DateTimeOffset.UtcNow;
                    row.ModifiedBy = identity.Code;
                    row.InstanceState = InstanceState.Deleted;

                    row.InstancesHistory.Add(new InstancesHistory
                    {
                        Date = row.ModifiedOn.Value,
                        User = row.ModifiedBy,
                        State = row.InstanceState,
                        Note = note
                    });
                }

                await _instancesRepository.SaveChangesAsync();
            }
            await _instanceCache.RemoveAsync(cacheObject.Code);
        }

        private async Task<GetPrototypeResponse> GetPrototype(long prototype, int version)
        {
            try
            {
                var httpResponse = await _client.PostAsync("Prototypes/Prototype/Open", new GetPrototypeRequest
                {
                    Id = prototype,
                    Version = version
                });

                var responseData = await httpResponse.Content.ReadAsStringAsync();
                if (httpResponse.StatusCode == HttpStatusCode.InternalServerError)
                {
                    var message = JsonConvert.DeserializeObject<ResponseMessage>(responseData);
                    throw new InstanceException(message.Messages.FirstOrDefault() ?? "Unable to retrieve the prototype");
                }
                var response = JsonConvert.DeserializeObject<GetPrototypeResponse>(responseData, _converters);
                return response;
            }
            catch (InstanceException e)
            {
                Logger.Error(e);
                throw;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Unable to retrieve the prototype");
                throw new InstanceException("Unable to retrieve the prototype", e);
            }
        }

        private async Task<InstanceCacheObject> CreateInstanceFromDatabase(long companyId, Common.Data.Instances instance)
        {
            var instanceObject = JsonConvert.DeserializeObject<Instance>(instance.Data, _converters);
            var cacheObject = new InstanceCacheObject
            {
                CompanyId = companyId,
                Instance = instanceObject,
                Code = instance.Code,
                Department = instance.Prototypes.Department,
                DepartmentName = instance.Prototypes.Departments.Name,
                PrototypeType = instance.Prototypes.PrototypeType,
                PrototypeTypeName = instance.Prototypes.PrototypeTypes.Name,
                Prototype = instance.Prototype,
                PrototypeName = instance.PrototypeName,
                PrototypeDescription = instance.PrototypeVersions.Description,
                Version = instance.Version,
                VersionState = instance.PrototypeVersions.VersionState,
                InstanceState = instance.InstanceState,
                CreatedBy = instance.CreatedBy,
                CreatedOn = instance.CreatedOn,
                Id = instance.ID,
                Year = instance.Year,
                Number = instance.Number,
                ModifiedOn = instance.ModifiedOn,
                ModifiedBy = instance.ModifiedBy
            };
            cacheObject.Instance.Setup(cacheObject, await _settingService.DecimalSeparatorAsync(companyId), await _settingService.TimeZoneAsync(companyId), false);
            return cacheObject;
        }

        #endregion

        #region security

        public void CheckViewPermission(IDefaultIdentity identity, InstanceCacheObject cacheObject)
        {
            if (!HasPermission(identity, "View", cacheObject.VersionState, cacheObject.Department, cacheObject.PrototypeType))
                throw new InstanceException("Access denied");
        }


        private bool HasPermission(IDefaultIdentity identity, string function, string PrototypeVersionState, long Department, long PrototypeType, string Tag = null)
        {
            return
                PrototypeVersionState.In(VersionState.Draft, VersionState.InSimulation)
                    && (
                        identity.HasPermission("Prototypes", "Preview", Department)
                        || identity.HasPermission("Prototypes", "Preview", Department, PrototypeType)
                    )
                || !PrototypeVersionState.In(VersionState.Draft, VersionState.InSimulation)
                && (
                    identity.HasPermission("Instances", function, Department, tag: Tag)
                    || identity.HasPermission("Instances", function, Department, PrototypeType, Tag)
                );
        }

        #endregion

        #region actions

        public async Task<string> GetExternalSourceRows(string code, IDefaultIdentity identity, string externalSourceCode, Dictionary<string, string> parameters, int page)
        {
            try
            {
                var cacheObject = await GetCacheObjectForEditAsync(identity, code, true);
                if (cacheObject.InstanceState != InstanceState.Draft && cacheObject.InstanceState != InstanceState.Progress)
                    throw new InstanceException("Instance state not valid");
                if (!HasPermission(identity, "Edit", cacheObject.VersionState, cacheObject.Department, cacheObject.PrototypeType))
                    throw new InstanceException("Access denied");
                if (!cacheObject.Instance.ExternalSources.ContainsKey(externalSourceCode))
                    throw new InstanceException("Invalid external source");

                await _instanceCache.TouchAsync(code);

                var token = identity.Code + ":" + Guid.NewGuid();
                await _redisCache.SetAsync($"Token:{token}", new DataRequest
                {
                    code = cacheObject.Instance.ExternalSources[externalSourceCode],
                    parameters = parameters,
                    page = page,
                    decimalSeparator = await _settingService.DecimalSeparatorAsync(identity.CompanyId),
                    timeZone = (await _commonRepository.GetCompanyAsync(identity.CompanyId)).TimeZone
                }, "S2S", new TimeSpan(0, 1, 0));
                var response = await _client.PostAsync("DataSources/Data/GetDataSourceRows", new TokenRequest { token = token });
                var responseData = await response.Content.ReadAsStringAsync();
                if (response.StatusCode != HttpStatusCode.OK)
                    throw new Exception(responseData);

                return JsonConvert.DeserializeObject<string>(responseData);
            }
            catch (InstanceException)
            {
                throw;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Unable to retrieve the external source data");
                throw new InstanceException("Unable to retrieve the external source data", e);
            }
        }

        public async Task<List<InstanceEventRequest.Email>> GetInstanceEditors(string code, IDefaultIdentity identity)
        {
            try
            {
                var cacheObject = await GetCacheObjectForEditAsync(identity, code, false);
                if (cacheObject.InstanceState != InstanceState.Draft && cacheObject.InstanceState != InstanceState.Progress)
                    throw new InstanceException("Instance state not valid");
                if (!HasPermission(identity, "Edit", cacheObject.VersionState, cacheObject.Department, cacheObject.PrototypeType))
                    throw new InstanceException("Access denied");

                await _instanceCache.TouchAsync(code);

                return (await _instancesRepository.GetInstanceEditors(cacheObject.Department, cacheObject.PrototypeType, cacheObject.VersionState)).Select(u => new InstanceEventRequest.Email
                {
                    Code = u.Code,
                    Description = u.Description,
                    Address = u.Email
                }).OrderBy(u => u.Code).ToList();
            }
            catch (InstanceException)
            {
                throw;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Unable to retrieve the instance editors");
                throw new InstanceException("Unable to retrieve the instance editors", e);
            }
        }

        public async Task<StorageFile> DownloadAttachmentAsync(IDefaultIdentity identity, string cacheObjectCode, string key)
        {
            if (key == null || !key.StartsWith(GetAttachmentKeyPrefix(identity, cacheObjectCode)))
                throw new InstanceException("Attachment key not valid");

            return await _storageService.GetAsync(_configurationService.InstancesStorageBucket, key);
        }

        public async Task ClearPendingInstances(CancellationToken cancellationToken = new CancellationToken())
        {
            var score = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - _configurationService.InstanceCloseTimeout * 60;
            var codes = await _instanceCache.GetInstancesInRangeAsync(stop: score);

            foreach (var code in codes)
                try
                {
                    var instance = await _instanceCache.GetAsync(code, false);
                    if (instance == null)
                        continue;

                    if (instance.NeedToSave)
                        switch (instance.InstanceState)
                        {
                            case InstanceState.Draft:
                                await _instanceCache.RemoveAsync(instance.Code);
                                break;
                            case InstanceState.Progress:
                                await SaveAsync(instance, null, true, false, false, instance.ModifiedBy, instance.ModifiedOn);
                                break;
                            default:
                                throw new InstanceException("Invalid state: " + instance.InstanceState);
                        }
                    else
                        await _instanceCache.RemoveAsync(code);
                }
                catch (Exception exception)
                {
                    Logger.Error(exception);
                }
        }

        public async Task<List<HistoryRow>> History(DefaultIdentity identity, long id)
        {
            var Instance = await _instancesRepository.GetAsync(identity.CompanyId, id);
            if (Instance == null)
                throw new InstanceException("Instance not found");
            if (!HasPermission(identity, "View", Instance.PrototypeVersions.VersionState, Instance.Prototypes.Department, Instance.Prototypes.PrototypeType))
                throw new UnauthorizedAccessException();

            return await Task.FromResult(Instance.InstancesHistory.OrderBy(h => h.ID).Select(h => new HistoryRow
            {
                ID = h.ID,
                Date = h.Date,
                User = $"{h.User} - {_commonRepository.GetUser(identity.CompanyId, h.User)?.Description ?? "(unknown user)"}",
                State = h.WorkFlowApplications == null ? h.State : h.WorkFlowApplications.FinalState,
                ProposedState = h.WorkFlowApplications == null ? null : InstanceState.Confirmed,
                Approved = h.WorkFlowApplications?.Approved,
                Note = h.Note,
                Steps = h.WorkFlowApplications?.WorkFlowApplicationSteps.OrderBy(s => s.Step).ThenBy(s => s.ID).Select(s => new HistoryStep
                {
                    ID = s.ID,
                    Step = s.Step + 1,
                    Date = s.Date,
                    User = s.User == null ? string.Empty : $"{s.User} - {s.UserDescription}",
                    Level = $"({string.Join(", ", s.WorkFlowApplicationStepTags.Select(t => t.Tag))})",
                    Reason = s.Reason,
                    Comment = s.Comment,
                    Approved = s.Approved,
                    AppointedUser = s.User == null ? string.Join(", ", _commonRepository.GetAppointedUsers(identity.CompanyId, "Instances", "Confirm", Instance.Prototypes.Department, Instance.Prototypes.PrototypeType, s.WorkFlowApplicationStepTags.Select(t => t.Tag)).Select(u => u.Code)) : null
                }).ToList()
            }).ToList());
        }

        public async Task InvalidateCacheAsync(DefaultIdentity identity, InvalidateCacheRequest request)
        {
            foreach (var code in request.Instances)
                try
                {
                    var instance = await _instanceCache.GetAsync(code, true);
                    if (instance != null)
                    {
                        if (instance.Prototype != request.Prototype || instance.Version != request.Version)
                            Logger.Warn($"Incongruent instance prototype and version: Received {request.Prototype}-{request.Version}, expected: {instance.Prototype}-{instance.Version}");

                        if (instance.NeedToSave)
                            await SaveAsync(
                                instance,
                                identity,
                                true,
                                false,
                                false,
                                instance.ModifiedBy,
                                instance.ModifiedOn
                            );
                    }
                }
                catch (InstanceException)
                {
                    // Ignored
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
        }

        #endregion

        #region events

        public async Task GetInstanceAsync(IDefaultIdentity identity, InstanceCacheObject cacheObject)
        {
            if (cacheObject.InstanceState == InstanceState.Progress)
            {
                Page page;
                if (cacheObject.Instance.Pages.Any() && (page = (Page)cacheObject.Instance.Pages.First()).Lock == null && HasPermission(identity, "Edit", cacheObject.VersionState, cacheObject.Department, cacheObject.PrototypeType))
                {
                    UnlockPages(cacheObject.Instance, identity.Code);
                    page.Lock = new Lock(identity.Code);
                    cacheObject.NeedToCache = true;
                }
            }

            await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
            {
                Company = identity.CompanyId,
                User = identity.Code,
                Action = TrailAction.InstanceOpened,
                Date = DateTimeOffset.UtcNow,
                CheckList = cacheObject.InstanceCode
            });
        }

        private static void UnlockPages(ICheckList instance, string userCode)
        {
            foreach (var Page in instance.Pages.Cast<Page>())
                if (Page.Lock?.User == userCode)
                    Page.Lock = null;
        }

        public async Task<InstanceCacheObject> HandleEventAsync(IDefaultIdentity identity, InstanceEventRequest request)
        {
            LockObject lockObject = null;

            double initialGetTime = 0, lockTime = 0, getTime = 0, handleTime = 0;

            try
            {
                var stopWatch = Stopwatch.StartNew();
                var cacheObject = await GetCacheObjectForEditAsync(identity, request.Code, false);
                CheckViewPermission(identity, cacheObject);
                stopWatch.Stop();
                initialGetTime = stopWatch.Elapsed.TotalMilliseconds;

                if (cacheObject == null)
                    throw new InstanceException("Instance not found");

                if (cacheObject.InstanceState == InstanceState.Progress)
                {
                    //Lock and reload the data to be sure it's the most recent one
                    stopWatch = Stopwatch.StartNew();
                    lockObject = await _instanceCache.LockAsync(request.Code);
                    stopWatch.Stop();
                    lockTime = stopWatch.Elapsed.TotalMilliseconds;
                    if (!lockObject.IsAcquired)
                        throw new InstanceException("Instance is currently being updated by another user");
                }

                stopWatch = Stopwatch.StartNew();
                cacheObject = await _instanceCache.GetAsync(request.Code, true);
                stopWatch.Stop();
                getTime = stopWatch.Elapsed.TotalMilliseconds;

                var lockUser = request.Type == "Signature" || request.Type == "SignatureCheck" ? request.Username : identity.Code; //A signer may sign also if the page is locked by the main user (the lock is for concurrency purpose, not for security)
                cacheObject.Instance.UpdateReadOnly(lockUser, cacheObject.InstanceState);

                stopWatch = Stopwatch.StartNew();

                List<Element> AffectedElements = new List<Element>();
                DependencyRelation.Properties? Property = null;
                switch (request.Type)
                {
                    case EventTypes.PageChange:
                        HandlePageChangeAsync(cacheObject, identity.Code, request.ItemId);
                        Property = DependencyRelation.Properties.ReadOnly;
                        break;
                    case EventTypes.ValueChange:
                        AffectedElements.Add(await HandleValueChangeAsync(cacheObject, identity.Code, request.ItemId, request.Value));
                        Property = DependencyRelation.Properties.Value;
                        break;
                    case EventTypes.ValueCopy:
                        AffectedElements.Add(await HandleValueCopyAsync(cacheObject, identity.Code, request.ItemId));
                        Property = DependencyRelation.Properties.Value;
                        break;
                    case EventTypes.Add:
                        (await HandleAddAsync(cacheObject, identity.Code, request.ItemId))?.GetAllChildren(AffectedElements);
                        break;
                    case EventTypes.Remove:
                        (await HandleRemoveAsync(cacheObject, identity.Code, request.ItemId))?.GetAllChildren(AffectedElements);
                        break;
                    case EventTypes.Up:
                        (await HandleMoveAsync(cacheObject, identity.Code, request.ItemId, -1))?.GetAllChildren(AffectedElements);
                        break;
                    case EventTypes.Down:
                        (await HandleMoveAsync(cacheObject, identity.Code, request.ItemId, 1))?.GetAllChildren(AffectedElements);
                        break;
                    case EventTypes.SignatureCheck:
                        HandleSignatureCheckAsync(cacheObject, request.Username, request.ItemId);
                        break;
                    case EventTypes.Signature:
                        AffectedElements.Add(await HandleSignatureAsync(cacheObject, identity, request));
                        Property = DependencyRelation.Properties.Validity;
                        break;
                    case EventTypes.SignatureCancelation:
                        AffectedElements.Add(await HandleSignatureCancellationAsync(cacheObject, identity, request));
                        Property = DependencyRelation.Properties.Validity;
                        break;
                    case EventTypes.SendEmail:
                        await HandleEmailSend(cacheObject, identity, request.ItemId, request.Emails);
                        break;
                    case EventTypes.UnlockPage:
                        HandleUnlockPageAsync(cacheObject, identity.Code, request.ItemId);
                        Property = DependencyRelation.Properties.ReadOnly;
                        break;
                    case EventTypes.CountDown:
                        break;
                    case EventTypes.SkipCountDown:
                        AffectedElements.Add(HandleSkipCountDown(cacheObject, request.Username, request.ItemId));
                        Property = DependencyRelation.Properties.CountDown;
                        break;
                    case EventTypes.Refresh:
                        break;
                    case EventTypes.Quit:
                        await HandleQuitAsync(cacheObject, identity);
                        Property = DependencyRelation.Properties.ReadOnly;
                        break;
                    case EventTypes.ExternalSourceRowChange:
                        await HandleExternalSourceRowChange(cacheObject, identity, request.ItemId, request.RowIndex, request.Row);
                        cacheObject.NeedToSave = true;
                        break;
                    case EventTypes.UploadAttachment:
                        AffectedElements.Add(await HandleUploadAttachment(cacheObject, identity, request.ItemId, request.Attachment));
                        Property = DependencyRelation.Properties.Validity;
                        break;
                    default:
                        throw new InstanceException($"Event type {request.Type} not handled.");
                }

                AffectedElements = AffectedElements.Where(x => x != null).ToList();
                if (AffectedElements.Any())
                {
                    var roots = new List<Tuple<DependencyNode, DependencyRelation.Properties>>();
                    foreach (var AffectedElement in AffectedElements)
                    {
                        if (AffectedElement is Item AffectedItem && AffectedItem.IterationItem)
                        { 
                            var IteratorModel = AffectedItem.FindIteratorModelItem();
                            if (Property.HasValue)
                                IteratorModel.MarkNodeAsChanged(cacheObject.Instance, roots, Property.Value);
                            else
                                IteratorModel.MarkAllAsChanged(roots);
                        }
                        else if (Property.HasValue)
                            AffectedElement.MarkNodeAsChanged(cacheObject.Instance, roots, Property.Value);
                        else
                            AffectedElement.MarkAllAsChanged(roots);
                    }
                    DependencyNode.UpdateNodes(cacheObject.Instance, roots);
                    cacheObject.Instance.UpdateReadOnly(lockUser ?? identity.Code, cacheObject.InstanceState);
                }
                else if (Property == DependencyRelation.Properties.ReadOnly)
                    cacheObject.Instance.UpdateReadOnly(lockUser, cacheObject.InstanceState);

                stopWatch.Stop();
                handleTime = stopWatch.Elapsed.TotalMilliseconds;

                return cacheObject;
            }
            catch (InstanceException)
            {
                throw;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error on handle event");
                throw new InstanceException("Error on handle event", e);
            }
            finally
            {
                Logger.Info(
                    $"Handle Event {request.Type} timings:{Environment.NewLine}" +
                    $"Initial get: {initialGetTime}{Environment.NewLine}" +
                    $"Lock: {lockTime}{Environment.NewLine}" +
                    $"Get: {getTime}{Environment.NewLine}" +
                    $"Handle: {handleTime}{Environment.NewLine}");
                lockObject?.Dispose();
            }
        }

        private static string GetAttachmentKeyPrefix(IDefaultIdentity identity, string cacheObjectCode)
            => $"{identity.CompanyId}/{cacheObjectCode}/";

        private void HandlePageChangeAsync(InstanceCacheObject cacheObject, string userCode, int? itemId)
        {
            if (!itemId.HasValue)
                throw new InstanceException("Item id not sent");

            var page = cacheObject.Instance.Pages.Count > itemId.Value ? cacheObject.Instance.Pages[itemId.Value] as Page : null;
            if (page == null)
                throw new InstanceException("Page not found");

            if (cacheObject.Instance.ReadOnly) return;

            UnlockPages(cacheObject.Instance, userCode);
            if (page.Lock == null)
                page.Lock = new Lock(userCode);
            TrackChanges(cacheObject, userCode);
        }

        private async Task<Element> HandleValueChangeAsync(InstanceCacheObject cacheObject, string userCode, int? itemId, string value)
        {
            if (!itemId.HasValue)
                throw new InstanceException("Item id not sent");

            var field = cacheObject.Instance.Items.ContainsKey(itemId.Value) ? cacheObject.Instance.Items[itemId.Value] as Field : null;
            if (field == null)
                throw new InstanceException($"Field {itemId} not found");

            if (field.ReadOnly)
                return null;

            if ((value ?? string.Empty) != (field.Value ?? string.Empty) && field.Validate(value))
            {
                string
                    oldFormattedValue = field.Value,
                    formattedValue = value;

                if (field.ValueTypeName == "DateTime")
                {
                    string TimeZone = (await _settingService.TimeZoneAsync(cacheObject.CompanyId));

                    if (field.LocalTimeZone)
                    {
                        if (DateTimeOffset.TryParseExact(oldFormattedValue, "yyyy-MM-dd HH:mm", null, System.Globalization.DateTimeStyles.AssumeUniversal, out var Date))
                            oldFormattedValue = Date.Format(TimeZone);
                        else
                            oldFormattedValue = string.Empty;

                        if (DateTimeOffset.TryParseExact(formattedValue, "yyyy-MM-dd HH:mm", null, System.Globalization.DateTimeStyles.AssumeUniversal, out Date))
                            formattedValue = Date.Format(TimeZone);
                        else
                            formattedValue = string.Empty;
                    }
                    else
                    {
                        var dateTimeOffset = DateTimeOffsetHelper.Parse(oldFormattedValue, TimeZone, out var Ambiguous, out var AmbiguousDateTimeOffsets, DateTimeOffsetHelper.DateFormat);
                        oldFormattedValue = dateTimeOffset?.Format(TimeZone) ?? string.Empty;

                        dateTimeOffset = DateTimeOffsetHelper.Parse(formattedValue, TimeZone, out Ambiguous, out AmbiguousDateTimeOffsets, DateTimeOffsetHelper.DateFormat);
                        formattedValue = dateTimeOffset?.Format(TimeZone) ?? string.Empty;
                    }
                }

                field.History.Add(new FieldRecord
                {
                    DateTime = DateTimeOffset.UtcNow,
                    User = userCode,
                    Value = formattedValue
                });

                await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
                {
                    Company = cacheObject.CompanyId,
                    User = userCode,
                    Action = TrailAction.InstanceValueChanged,
                    Date = DateTimeOffset.UtcNow,
                    CheckList = cacheObject.InstanceCode,
                    Information = JsonConvert.SerializeObject(new
                    {
                        ID = itemId,
                        OldValue = oldFormattedValue,
                        NewValue = formattedValue
                    })
                });
                field.Value = value;

                TrackChanges(cacheObject, userCode);
            }

            return field;
        }

        private async Task<Element> HandleValueCopyAsync(InstanceCacheObject cacheObject, string userCode, int? itemId)
        {
            if (!itemId.HasValue)
                throw new InstanceException("Item id not sent");

            var field = cacheObject.Instance.Items.ContainsKey(itemId.Value) ? cacheObject.Instance.Items[itemId.Value] as Field : null;
            if (field == null)
                throw new InstanceException($"Field {itemId} not found");

            if (field.ReadOnly)
                return null;

            string value = null;

            if (field.IterationItem && field.Iteration.Index > 1)
            {
                var previousIteration = field.Iteration.Group.Iterations.Where(i => !i.Removed).ElementAt(field.Iteration.Index - 2);
                var ItemIndex = field.Iteration.Items.Contains(field) ? field.Iteration.Items.IndexOf(field) : -1;
                var previousField = ItemIndex < 0 ? null : (Field)previousIteration.Items[field.Iteration.Items.IndexOf(field)];
                value = previousField?.Value ?? string.Empty;
            }

            if ((value ?? string.Empty) != (field.Value ?? string.Empty) && field.Validate(value))
            {
                string
                    oldFormattedValue = field.Value,
                    formattedValue = value;

                if (field.ValueTypeName == "DateTime")
                {
                    string TimeZone = (await _settingService.TimeZoneAsync(cacheObject.CompanyId));

                    if (field.LocalTimeZone)
                    {
                        if (DateTimeOffset.TryParseExact(oldFormattedValue, "yyyy-MM-dd HH:mm", null, System.Globalization.DateTimeStyles.AssumeUniversal, out var Date))
                            oldFormattedValue = Date.Format(TimeZone);
                        else
                            oldFormattedValue = string.Empty;

                        if (DateTimeOffset.TryParseExact(formattedValue, "yyyy-MM-dd HH:mm", null, System.Globalization.DateTimeStyles.AssumeUniversal, out Date))
                            formattedValue = Date.Format(TimeZone);
                        else
                            formattedValue = string.Empty;
                    }
                    else
                    {
                        var dateTimeOffset = DateTimeOffsetHelper.Parse(oldFormattedValue, TimeZone, out var Ambiguous, out var AmbiguousDateTimeOffsets, DateTimeOffsetHelper.DateFormat);
                        oldFormattedValue = dateTimeOffset?.Format(TimeZone) ?? string.Empty;

                        dateTimeOffset = DateTimeOffsetHelper.Parse(formattedValue, TimeZone, out Ambiguous, out AmbiguousDateTimeOffsets, DateTimeOffsetHelper.DateFormat);
                        formattedValue = dateTimeOffset?.Format(TimeZone) ?? string.Empty;
                    }
                }

                field.History.Add(new FieldRecord
                {
                    DateTime = DateTimeOffset.UtcNow,
                    User = userCode,
                    Value = formattedValue
                });
                await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
                {
                    Company = cacheObject.CompanyId,
                    User = userCode,
                    Action = TrailAction.InstanceValueCopied,
                    Date = DateTimeOffset.UtcNow,
                    CheckList = cacheObject.InstanceCode,
                    Information = JsonConvert.SerializeObject(new
                    {
                        ID = itemId,
                        OldValue = oldFormattedValue,
                        NewValue = formattedValue
                    })
                });
                field.Value = value;

                TrackChanges(cacheObject, userCode);
            }

            return field;
        }

        private async Task<Iteration> HandleAddAsync(InstanceCacheObject cacheObject, string userCode, int? itemId)
        {
            if (!itemId.HasValue)
                throw new InstanceException("Item id not sent");

            var group = cacheObject.Instance.Items.ContainsKey(itemId.Value) ? cacheObject.Instance.Items[itemId.Value] as Group : null;
            if (group == null)
                throw new InstanceException("Group not found");

            if (!group.Iterator || group.DisableAdd)
                throw new InstanceException("Invalid operation");

            Iteration Iteration = null;
            if (!group.ReadOnly)
            {
                Iteration = group.AddIteration();
                group.SetupIterationsIndexes();
                await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
                {
                    Company = cacheObject.CompanyId,
                    User = userCode,
                    Action = TrailAction.InstanceIterationAdded,
                    Date = DateTimeOffset.UtcNow,
                    CheckList = cacheObject.InstanceCode,
                    Information = JsonConvert.SerializeObject(new { ID = Iteration.Id.ToString() })
                });
                TrackChanges(cacheObject, userCode);
            }

            return Iteration;
        }

        private async Task<Iteration> HandleRemoveAsync(InstanceCacheObject cacheObject, string userCode, int? itemId)
        {
            if (!itemId.HasValue)
                throw new InstanceException("Item id not sent");

            var iteration = cacheObject.Instance.Items.ContainsKey(itemId.Value) ? cacheObject.Instance.Items[itemId.Value] as Iteration : null;
            if (iteration == null)
                throw new InstanceException("Group not found");

            if (iteration.Group.DisableRemove)
                throw new InstanceException("Invalid operation");

            if (iteration.ReadOnly) return null;

            var group = (Group)iteration.Group;
            group.Remove(iteration);
            group.SetupIterationsIndexes();
            await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
            {
                Company = cacheObject.CompanyId,
                User = userCode,
                Action = TrailAction.InstanceIterationDeleted,
                Date = DateTimeOffset.UtcNow,
                CheckList = cacheObject.InstanceCode,
                Information = JsonConvert.SerializeObject(new { ID = itemId.ToString() })
            });
            TrackChanges(cacheObject, userCode);
            return iteration;
        }

        private async Task<Iteration> HandleMoveAsync(InstanceCacheObject cacheObject, string userCode, int? itemId, int direction)
        {
            if (!itemId.HasValue)
                throw new InstanceException("Item id not sent");

            var iteration = cacheObject.Instance.Items.ContainsKey(itemId.Value) ? cacheObject.Instance.Items[itemId.Value] as Iteration : null;
            if (iteration == null)
                throw new InstanceException("Group not found");

            if (iteration.Group.DisableMove)
                throw new InstanceException("Invalid operation");

            if (iteration.ReadOnly) return null;

            var group = (Group)iteration.Group;
            group.Move(iteration, direction);
            group.SetupIterationsIndexes();
            await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
            {
                Company = cacheObject.CompanyId,
                User = userCode,
                Action = direction == 1 ? TrailAction.InstanceIterationMovedDown : TrailAction.InstanceIterationMovedUp,
                Date = DateTimeOffset.UtcNow,
                CheckList = cacheObject.InstanceCode,
                Information = JsonConvert.SerializeObject(new { ID = itemId.ToString() })
            });
            TrackChanges(cacheObject, userCode);
            return iteration;
        }

        private void HandleSignatureCheckAsync(InstanceCacheObject cacheObject, string userCode, int? itemId)
        {
            if (!itemId.HasValue)
                throw new InstanceException("Item id not sent");

            var signature = cacheObject.Instance.Items.ContainsKey(itemId.Value) ? cacheObject.Instance.Items[itemId.Value] as Signature : null;
            if (signature == null)
                throw new InstanceException("Signature not found");

            if (signature.ReadOnly)
                throw new InstanceException("Invalid operation");

            if (signature.Signed)
                throw new Exception("The signature is already signed");

            signature.TrySign(userCode);
        }

        private async Task<Element> HandleSignatureAsync(InstanceCacheObject cacheObject, IDefaultIdentity identity, InstanceEventRequest request)
        {
            try
            {
                if (!request.ItemId.HasValue)
                    throw new InstanceException("Item id not sent");

                var signature = cacheObject.Instance.Items.ContainsKey(request.ItemId.Value) ? cacheObject.Instance.Items[request.ItemId.Value] as Signature : null;
                if (signature == null)
                    throw new InstanceException("Signature not found");

                if (signature.ReadOnly)
                    throw new InstanceException("Invalid operation");

                if (signature.Signed)
                    throw new Exception("The signature is already signed");

                if (request.Simple && string.IsNullOrEmpty(request.Password))
                    throw new InstanceException("Provide your password");
                if (!request.Simple && string.IsNullOrEmpty(request.Pin))
                    throw new InstanceException("Provide your PIN");

                signature.TrySign(identity.Code);

                try
                {
                    if (request.Simple)
                        await _authenticationService.AuthenticateAsync(identity.Code, request.Password);
                    else
                        await _authenticationService.AuthenticateWithPINAsync(request.Pin);
                }
                catch
                {
                    throw new InstanceException("Authentication failed");
                }

                if (
                    !string.IsNullOrEmpty(signature.SignatureLevel)
                    && !HasPermission(identity, "Edit", cacheObject.VersionState, cacheObject.Department, cacheObject.PrototypeType, signature.SignatureLevel)
                )
                    throw new InstanceException($"The signature must be applied by an user with level {signature.SignatureLevel}");

                var Now = DateTimeOffset.UtcNow;
                cacheObject.ModifiedBy = identity.Code;
                cacheObject.ModifiedOn = Now;
                signature.Sign(identity.Code, identity.Description, Now);
                if (cacheObject.InstanceState == InstanceState.Progress)
                    await SaveAsync(cacheObject, identity, false, false, true, cacheObject.ModifiedBy, cacheObject.ModifiedOn);
                TrackChanges(cacheObject, request.Username);

                await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
                {
                    Company = identity.CompanyId,
                    User = identity.Code,
                    Action = TrailAction.InstanceSignatureIssued,
                    Date = DateTimeOffset.UtcNow,
                    CheckList = cacheObject.InstanceCode,
                    Information = JsonConvert.SerializeObject(new { ID = request.ItemId.ToString() })
                });

                return signature;
            }
            catch (Exception e)
            {
                var information = new Dictionary<string, string>
                {
                    { "Id", request.ItemId.ToString() },
                    { "ErrorMessage", e.Message }
                };
                await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
                {
                    Company = identity.CompanyId,
                    User = identity.Code,
                    Action = TrailAction.InstanceSignatureFailed,
                    Date = DateTimeOffset.UtcNow,
                    CheckList = cacheObject.InstanceCode,
                    Information = information.ToJson(new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None })
                });

                throw;
            }
        }

        private async Task<Element> HandleSignatureCancellationAsync(InstanceCacheObject cacheObject, IDefaultIdentity identity, InstanceEventRequest request)
        {
            try
            {
                if (!request.ItemId.HasValue)
                    throw new InstanceException("Item id not sent");

                var signature = cacheObject.Instance.Items.ContainsKey(request.ItemId.Value) ? cacheObject.Instance.Items[request.ItemId.Value] as Signature : null;
                if (signature == null)
                    throw new InstanceException("Signature not found");

                if (!signature.Signed)
                    throw new Exception("The signature is not signed");

                if (string.IsNullOrEmpty(request.Password))
                    throw new InstanceException("Provide your password");

                if (string.IsNullOrWhiteSpace(request.Note))
                    throw new InstanceException("Provide a cancellation note");

                if (cacheObject.Instance.ReadOnly)
                    throw new InstanceException("Invalid operation");

                try
                {
                    await _authenticationService.AuthenticateAsync(identity.Code, request.Password);
                }
                catch
                {
                    throw new InstanceException("Authentication failed");
                }

                if (!HasPermission(identity, "Unsign", cacheObject.VersionState, cacheObject.Department, cacheObject.PrototypeType, signature.SignatureLevel))
                    throw new InstanceException($"The signature can be canceled by an user with function Unsign and level {signature.SignatureLevel}");

                signature.Unsign(identity.Code, identity.Description, DateTimeOffset.UtcNow, request.Note, cacheObject.InstanceState);
                var Now = DateTimeOffset.UtcNow;
                cacheObject.ModifiedBy = identity.Code;
                cacheObject.ModifiedOn = Now;
                if (cacheObject.InstanceState == InstanceState.Progress)
                    await SaveAsync(cacheObject, identity, false, false, true, cacheObject.ModifiedBy, cacheObject.ModifiedOn);
                TrackChanges(cacheObject, request.Username);

                await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
                {
                    Company = identity.CompanyId,
                    User = identity.Code,
                    Action = TrailAction.InstanceSignatureCanceled,
                    Date = DateTimeOffset.UtcNow,
                    CheckList = cacheObject.InstanceCode,
                    Information = JsonConvert.SerializeObject(new
                    {
                        ID = request.ItemId,
                        request.Note
                    })
                });

                return signature;
            }
            catch (Exception e)
            {
                await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
                {
                    Company = identity.CompanyId,
                    User = identity.Code,
                    Action = TrailAction.InstanceSignatureCancelationFailed,
                    Date = DateTimeOffset.UtcNow,
                    CheckList = cacheObject.InstanceCode,
                    Information = JsonConvert.SerializeObject(new
                    {
                        ID = request.ItemId,
                        request.Note,
                        ErrorMessage = e.Message
                    })
                });
                throw;
            }
        }

        private async Task HandleEmailSend(InstanceCacheObject cacheObject, IDefaultIdentity identity, int? itemId, List<InstanceEventRequest.Email> Emails)
        {
            if (!itemId.HasValue)
                throw new InstanceException("Item id not sent");

            var signature = cacheObject.Instance.Items.ContainsKey(itemId.Value) ? cacheObject.Instance.Items[itemId.Value] as Signature : null;
            if (signature == null)
                throw new InstanceException("Signature not found");

            if (!signature.Signed)
                throw new Exception("The signature is not signed");

            var message = await _settingService.InstanceMessageAsync(identity.CompanyId);
            var webApplicationUrl = _configurationService.WebApplicationUrl;
            var checkListURL = $"{webApplicationUrl}/Instances/Detail/{cacheObject.Code}";
            //var checklistLabel = $"{cacheObject.PrototypeName}.{cacheObject.Version}.{(cacheObject.Year > 0 ? cacheObject.Year.ToString() : cacheObject.VersionState == VersionState.Draft ? "Test" : "SE")}.{cacheObject.Number?.ToString() ?? "?"}";
            foreach (var Email in Emails)
                await _queueService.SendObjectAsync(QueueName.Email,
                    new EmailMessage(identity.CompanyId, "Signature notification", message,
                        Email.Address, new List<Tuple<string, string>> {
                            new Tuple<string, string> ("User", Email.Description),
                            new Tuple<string, string> ("CheckListCode", $"<a href='{checkListURL}' target='_blank'>{cacheObject.InstanceCode}</a>")
                    })
                    {
                        TrailMessage = new EmailTrailMessage(identity.Code, cacheObject.InstanceCode, new EmailTrailMessage.EmailTrailInformation
                        {
                            Id = itemId.Value.ToString()
                        })
                    });
        }

        private void HandleUnlockPageAsync(InstanceCacheObject cacheObject, string userCode, int? itemId)
        {
            if (!itemId.HasValue)
                throw new InstanceException("Item id not sent");

            var page = cacheObject.Instance.Pages.Count > itemId.Value ? cacheObject.Instance.Pages[itemId.Value] as Page : null;
            if (page == null)
                throw new InstanceException("Page not found");

            page.Lock = new Lock(userCode);

            TrackChanges(cacheObject, userCode);
        }

        private Element HandleSkipCountDown(InstanceCacheObject cacheObject, string userCode, int? itemId)
        {
            if (!itemId.HasValue)
                throw new InstanceException("Item id not sent");

            var element =
                itemId > 0 ?
                    cacheObject.Instance.Items.ContainsKey(itemId.Value) ? (ICheckListElement)cacheObject.Instance.Items[itemId.Value] : null :
                    cacheObject.Instance.Pages.Count > -itemId.Value ? cacheObject.Instance.Pages[-itemId.Value] : null;

            if (element == null)
                throw new InstanceException("Element not found");

            if (!cacheObject.VersionState.In(VersionState.Draft, VersionState.InSimulation))
                throw new InvalidOperationException("A count down can be skipped only for preview and simulation instances");

            element.CountDownToCode = null;
            TrackChanges(cacheObject, userCode);

            return (Element)element;
        }

        private async Task HandleQuitAsync(InstanceCacheObject cacheObject, IDefaultIdentity identity)
        {
            try
            {
                if (!cacheObject.Instance.ReadOnly)
                {
                    UnlockPages(cacheObject.Instance, identity.Code);
                    TrackChanges(cacheObject, identity.Code);
                }

                await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
                {
                    Company = identity.CompanyId,
                    User = identity.Code,
                    Action = TrailAction.InstanceQuitted,
                    Date = DateTimeOffset.UtcNow,
                    CheckList = cacheObject.InstanceCode
                });
            }
            catch (InstanceException)
            {
                throw;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Unable to quit the instance");
                throw new InstanceException("Unable to quit the instance", e);
            }
        }

        private void TrackChanges(InstanceCacheObject cacheObject, string userCode)
        {
            foreach (Page Page in cacheObject.Instance.Pages)
                if (Page.Lock != null && Page.Lock.User == userCode)
                    Page.Lock.LastUpdate = DateTimeOffset.Now;

            cacheObject.NeedToSave = true;
            cacheObject.NeedToCache = true;
            cacheObject.ModifiedBy = userCode;
            cacheObject.ModifiedOn = DateTimeOffset.UtcNow;
        }

        private async Task HandleExternalSourceRowChange(InstanceCacheObject cacheObject, IDefaultIdentity identity, int? itemId, int? rowIndex, Dictionary<string, string> row)
        {
            if (!itemId.HasValue)
                throw new InstanceException("Item id not sent");

            var field = cacheObject.Instance.Items.ContainsKey(itemId.Value) ? cacheObject.Instance.Items[itemId.Value] as Field : null;
            if (field == null)
                throw new InstanceException($"Field {itemId} not found");

            if (field.ReadOnly)
                return;

            if (cacheObject.Instance.ExternalSourceRows == null) cacheObject.Instance.ExternalSourceRows = new PascalCaseDictionary<string, PascalCaseDictionary<string, string>>();
            cacheObject.Instance.ExternalSourceRows[field.ExternalSourceName] = new PascalCaseDictionary<string, string>(row);

            await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
            {
                Company = cacheObject.CompanyId,
                User = identity.Code,
                Action = TrailAction.InstanceExternalSourceRowChanged,
                Date = DateTimeOffset.UtcNow,
                CheckList = cacheObject.InstanceCode,
                Information = JsonConvert.SerializeObject(new
                {
                    ID = itemId,
                    Values = row
                })
            });

            TrackChanges(cacheObject, identity.Code);
        }

        private async Task<Element> HandleUploadAttachment(
            InstanceCacheObject cacheObject,
            IDefaultIdentity identity,
            int? itemId,
            HttpFile attachment
        )
        {
            if (!itemId.HasValue)
                throw new InstanceException("Item id not sent");

            var item = cacheObject.Instance.Items.ContainsKey(itemId.Value) ? cacheObject.Instance.Items[itemId.Value] as Attachment : null;
            if (item == null)
                throw new InstanceException($"Attachment {itemId} not found");

            if (item.ReadOnly)
                return null;

            if (attachment == null)
                throw new InstanceException("No attachment has been uploaded");

            var extension = Path.GetExtension(attachment.FileName);
            if (item.FileType == Attachment.FileTypes.Images)
            {
                if (!attachment.MediaType.StartsWith("image/"))
                    throw new InstanceException("The attachment has to be an image");

                attachment.Buffer = ImageAdapter.RotateAndCompressImage(attachment.Buffer);
            }
            else if (item.FileType == Attachment.FileTypes.Pdf)
            {
                if (extension != ".pdf")
                    throw new InstanceException("The attachment has to be a pdf document");
            }
            else
                throw new InstanceException("File type not supported");

            var maxSize = await _settingService.InstancesAttachmentMaxSizeBytesAsync(identity.CompanyId);
            if (attachment.Buffer.Length > maxSize)
                throw new InstanceException($"The file exceed the file size limit of {maxSize.BytesCountToString()}");

            var key = $"{GetAttachmentKeyPrefix(identity, cacheObject.Code)}{itemId.Value}{extension}";
            using (var memoryStream = new MemoryStream(attachment.Buffer))
            {
                var success = await _storageService.PutAsync(_configurationService.InstancesStorageBucket, key, memoryStream);
                if (!success)
                    throw new InstanceException("Impossible to upload the attachment to the storage");
            }

            item.Key = key;

            await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
            {
                Company = cacheObject.CompanyId,
                User = identity.Code,
                Action = TrailAction.InstanceValueChanged,
                Date = DateTimeOffset.UtcNow,
                CheckList = cacheObject.InstanceCode,
                Information = JsonConvert.SerializeObject(new
                {
                    ID = itemId,
                    attachment.FileName
                })
            });

            TrackChanges(cacheObject, identity.Code);

            return item;
        }

        #endregion
    }
}