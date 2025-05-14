using System;
using System.Linq;
using System.Net;
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
using CheckListManager.Common.Logic.Models.Expressions;
using CheckListManager.Common.Logic.Models.ExternalSources;
using CheckListManager.Instances.Data.Models;

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
        private readonly IEmailServiceBase _emailService;

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
            IStorageService storageService,
            IEmailServiceBase emailService
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
            _emailService = emailService;
        }

        #region cache object task

        public async Task<InstanceCacheObject> GetCacheObjectForDownloadAsync(long company, string code, bool ignoreCache)
        {
            var cacheObject = await GetCacheObjectAsync(company, code, true, ignoreCache);
            cacheObject.Instance.UpdateReadOnly(null, cacheObject.InstanceState, false);
            return cacheObject;
        }

        public async Task<InstanceCacheObject> GetCacheObjectForEditAsync(IDefaultIdentity identity, string code, bool includeData)
        {
            var cacheObject = await GetCacheObjectAsync(identity.CompanyId, code, includeData, false);
            if (includeData)
            {
                cacheObject.Instance.ReadOnly =
                    !cacheObject.InstanceState.In(InstanceState.Draft, InstanceState.Progress)
                    || !HasPermission(identity, "Edit", cacheObject.VersionState, cacheObject.Department, cacheObject.PrototypeType)
                    || cacheObject.Year == 0 && !cacheObject.VersionState.In(VersionState.Draft, VersionState.InSimulation);
                cacheObject.Instance.UpdateReadOnly(identity.Code, cacheObject.InstanceState, false);
            }
            return cacheObject;
        }

        private async Task<InstanceCacheObject> GetCacheObjectAsync(long company, string code, bool includeData, bool ignoreCache)
        {
            var cacheObject = ignoreCache ? default(InstanceCacheObject) : await _instanceCache.GetAsync(code, includeData);
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

        private async Task SaveCacheObjectAsync(InstanceCacheObject cacheObject)
        {
            if (cacheObject.NeedToSaveExpressionValues && cacheObject.ID.HasValue)
            {
                _instancesRepository.SaveExpressionValues(cacheObject.ID.Value, CreateExpressionValues(cacheObject));
                await _instancesRepository.SaveChangesAsync();
            }
            if (!cacheObject.Instance.CheckVersion(null, true) || cacheObject.NeedToCache)
                await _instanceCache.SetAsync(cacheObject);
            else
                await _instanceCache.TouchAsync(cacheObject.Code);
        }

        #endregion

        #region record tasks

        public async Task<GetPrototypeResponse> CheckCreationGrantsAsync(IDefaultIdentity identity, long prototype, int? version)
        {
            if (!version.HasValue)
                version = await _instancesRepository.GetActivePrototypeVersion(prototype);
            if (!version.HasValue)
                throw new InstanceException($"The prototype {prototype} has no active version");

            var source = await GetPrototype(prototype, version.Value);
            if (!source.State.In(VersionState.Draft, VersionState.InSimulation, VersionState.Active))
                throw new InstanceException($"Invalid state of the prototype version {prototype}.{version}: {source.State}");

            if (!HasPermission(identity, "Create", source.State, source.Department, source.PrototypeType))
                throw new InstanceException("Access denied");

            return source;
        }

        private void AddSimulationNote(Instance instance)
        {
            var SimulationPage = new Page()
            {
                Description = "Simulation notes",
                Groups = new List<ICheckListGroup>
                {
                    new Group
                    {
                        Description = "Simulation notes",
                        Items = new List<ICheckListItem>
                        {
                            new Field
                            {
                                Description = "Reason",
                                LinesCount = 5
                            },
                            new Field
                            {
                                Description = "Result",
                                LinesCount = 5
                            }

                        }
                    }
                }
            };
            instance.Pages.Add(SimulationPage);
        }

        public async Task<InstanceCacheObject> CreateInstanceAsync(IDefaultIdentity identity, bool saveCache, long prototype, int? version, Dictionary<string, string> parameters, string parentInstanceCode, int? parentInstanceLinkID)
        {
            try
            {
                var source = await CheckCreationGrantsAsync(identity, prototype, version);
                if (source.State == VersionState.InSimulation)
                    AddSimulationNote(source.CheckList);

                var cacheObject = new InstanceCacheObject
                {
                    CompanyId = identity.CompanyId,
                    Instance = source.CheckList,
                    Code = $"{prototype:X}-{source.Version.Value:X}-{Guid.NewGuid()}",
                    Department = source.Department,
                    DepartmentName = source.DepartmentName,
                    PrototypeType = source.PrototypeType,
                    PrototypeTypeName = source.PrototypeTypeName,
                    Prototype = prototype,
                    PrototypeName = source.Name,
                    PrototypeDescription = source.Description,
                    Version = source.Version.Value,
                    VersionState = source.State,
                    InstanceState = InstanceState.Draft,
                    CreatedBy = identity.Code,
                    CreatedOn = DateTimeOffset.UtcNow,
                    NeedToCache = true
                };

                cacheObject.Instance.ParentInstanceCode = parentInstanceCode;
                cacheObject.Instance.ParentInstanceLinkID = parentInstanceLinkID;
                cacheObject.Instance.Setup(cacheObject, await _settingService.DecimalSeparatorAsync(cacheObject.CompanyId), await _settingService.TimeZoneAsync(cacheObject.CompanyId), true);
                if (parameters != null)
                    foreach (var parameter in parameters)
                        if (cacheObject.Instance.Elements.TryGetValue(parameter.Key, out ICheckListElement element) && element is ICheckListItem item)
                            await HandleValueChangeAsync(cacheObject, identity.Code, item.Id, parameter.Value);

                cacheObject.Instance.UpdateAll(cacheObject);
                cacheObject.Instance.UpdateReadOnly(identity.Code, cacheObject.InstanceState, false);

                if (saveCache) await SaveCacheObjectAsync(cacheObject);

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

        private void CheckInvalidItems(ICheckListItem Item, List<string> InvalidItems)
        {
            if (!Item.Removed && !Item.IteratorModelItem && Item is ICheckListValidityElement checkListValidityElement && !checkListValidityElement.Valid)
                InvalidItems.Add(Item.ToString());
            if (Item is ICheckListGroup checkListGroup)
                foreach (var Child in checkListGroup.Items)
                    CheckInvalidItems(Child, InvalidItems);
        }

        private async Task UpdateJSONFieldsAsync(InstanceCacheObject cacheObject)
        {
            var itemValues = cacheObject.Instance.Items.Where(i => !i.Value.Removed && !i.Value.IterationItem && !i.Value.IteratorModelItem).ToDictionary(k => k.Key, v => ((Item)v.Value).GetViewModel(true));
            var itemsByLabel = new PascalCaseDictionary<string, dynamic>(cacheObject.Instance.Items.Where(i => !string.IsNullOrWhiteSpace(i.Value.Description) && itemValues.Keys.Contains(i.Key)).GroupBy(k => k.Value.Description, v => itemValues[v.Key]).ToDictionary(k => k.Key, v => v.Last()));
            var itemsByCode = new PascalCaseDictionary<string, dynamic>(cacheObject.Instance.Items.Where(i => !string.IsNullOrWhiteSpace(i.Value.Code) && itemValues.Keys.Contains(i.Key)).GroupBy(k => k.Value.Code, v => itemValues[v.Key]).ToDictionary(k => k.Key, v => v.Last()));
            var expressions = new PascalCaseDictionary<string, dynamic>(cacheObject.Instance.Expressions.ToDictionary(k => k.Key, v => v.Value.GetViewModel(cacheObject.Instance.DecimalSeparator)));
            await _instancesRepository.UpdateJSONFieldsAsync(cacheObject.ID.Value, cacheObject.CompanyId, JsonConvert.SerializeObject(itemsByCode), JsonConvert.SerializeObject(itemsByLabel), JsonConvert.SerializeObject(expressions));
        }

        public async Task<InstanceCacheObject> OpenInstanceAsync(InstanceCacheObject cacheObject, string code, IDefaultIdentity identity)
        {
            try
            {
                if (cacheObject == null)
                    cacheObject = await GetCacheObjectForEditAsync(identity, code, true);

                if (cacheObject.ID.HasValue)
                    throw new InstanceException("Instance already saved");
                if (cacheObject.InstanceState != InstanceState.Draft)
                    throw new InstanceException("Instance state not valid");
                if (!HasPermission(identity, "Create", cacheObject.VersionState, cacheObject.Department, cacheObject.PrototypeType))
                    throw new InstanceException("Access denied");

                var InvalidItems = new List<string>();
                CheckInvalidItems(cacheObject.Instance.Header, InvalidItems);
                if (InvalidItems.Any())
                    throw new InstanceException($"The following items are not valid: {string.Join(", ", InvalidItems)}");

                cacheObject.InstanceState = InstanceState.Progress;
                cacheObject.Instance.InProgressOn = DateTime.UtcNow;

                var row = new Common.Data.Instances
                {
                    Company = identity.CompanyId,
                    Data = JsonConvert.SerializeObject(cacheObject.Instance, _converters),
                    Code = code,
                    Description = cacheObject.Instance.DescriptionFormula.Value,
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

                    cacheObject.ID = row.ID;
                    cacheObject.Year = row.Year;
                    cacheObject.Number = row.Number;
                    if (cacheObject.Instance.Pages.Any()) ((Page)cacheObject.Instance.Pages[0]).Lock = new Lock(identity.Code);
                    TrackChanges(cacheObject, identity.Code);

                    cacheObject.Instance.UpdateInstanceRuntimeValues(cacheObject);
                    cacheObject.Instance.Update(DependencyRelation.Properties.Formula);

                    var Roots = new HashSet<Tuple<DependencyNode, DependencyRelation.Properties>>();
                    cacheObject.Instance.MarkNodeAsChanged(cacheObject.Instance, Roots, DependencyRelation.Properties.StateChange);
                    DependencyNode.UpdateNodes(cacheObject.Instance, Roots);

                    cacheObject.Instance.UpdateReadOnly(identity.Code, InstanceState.Progress, false);

                    _instancesRepository.SaveChildren(row.ID, CreateTags(cacheObject), CreateExpressionValues(cacheObject));

                    foreach (var Relation in cacheObject.Instance.ChildInstances)
                        _instancesRepository.AddRelation(row.ID, Relation.Key, Relation.Value);

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
                    CheckListCode = cacheObject.InstanceCode,
                    CheckListID = cacheObject.ID
                });

                await UpdateParentInstance(cacheObject, identity, false);
                await UpdateJSONFieldsAsync(cacheObject);
                await SaveCacheObjectAsync(cacheObject);

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
                if (!HasPermission(identity, "Edit", cacheObject.VersionState, cacheObject.Department, cacheObject.PrototypeType))
                    throw new InstanceException("Access denied");

                if (cacheObject.NeedToSave)
                    await UpdateDataBaseAsync(cacheObject, identity, false, false, false, cacheObject.ModifiedBy, cacheObject.ModifiedOn);
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
                var cacheObject = await GetCacheObjectForEditAsync(identity, code, false);
                if (!HasPermission(identity, "Edit", cacheObject.VersionState, cacheObject.Department, cacheObject.PrototypeType))
                    throw new InstanceException("Access denied");

                cacheObject =
                    await UpdateDataBaseAsync(cacheObject, identity, true, true, false, identity.Code, DateTimeOffset.UtcNow);
                cacheObject.Instance.UpdateReadOnly(identity.Code, InstanceState.Completed, false);

                await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
                {
                    Company = identity.CompanyId,
                    User = identity.Code,
                    Action = TrailAction.InstanceStateChanged,
                    Date = DateTimeOffset.UtcNow,
                    CheckListCode = cacheObject.InstanceCode,
                    CheckListID = cacheObject.ID,
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
                var cacheObject = await GetCacheObjectForEditAsync(identity, code, true);
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
                    CheckListCode = cacheObject.InstanceCode,
                    CheckListID = cacheObject.ID,
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
                    {
                        var Description = Item.Description.Length > 47 ? Item.Description.Substring(0, 47) + "..." : Item.Description;
                        Tags[Description] = new InstanceTags { Instance = InstanceID, Tag = Description, Value = (Item.TypeName == "Field" ? ((Field)Item).Value : ((ICheckListSignature)Item).SignedBy) ?? string.Empty };
                    }
        }

        private IEnumerable<InstanceTags> CreateTags(InstanceCacheObject cacheObject)
        {
            var Tags = new Dictionary<string, InstanceTags>();
            AddTags(Tags, cacheObject.ID.Value, cacheObject.Instance?.Header);
            return Tags.Values;
        }

        private IEnumerable<InstanceExpressionValues> CreateExpressionValues(InstanceCacheObject cacheObject)
        {
            var Result = cacheObject.Instance.Expressions.Where(
                Expression =>
                    !Expression.Value.Value.IsNull
                    &&
                    (
                        Expression.Value.Exception && (
                            Expression.Value.Value.Type == DataType.Boolean && Expression.Value.Value.BooleanValue
                            || (Expression.Value.Value.Type == DataType.Date || Expression.Value.Value.Type == DataType.DateTime) && Expression.Value.Value.DateTimeValue < DateTimeOffset.UtcNow
                        )
                        || Expression.Value.Unique != Common.Logic.Models.Expressions.Expression.Uniqueness.no && Expression.Value.Value.Type != DataType.Array
                        || Expression.Value.Notification && (Expression.Value.Value.Type != DataType.Boolean || Expression.Value.Value.BooleanValue)
                    )
                ).Select(e => new InstanceExpressionValues
                {
                    Instance = cacheObject.ID.Value,
                    Code = e.Key,
                    Description = e.Value.Description,
                    Value =
                        e.Value.Value.Type == DataType.Boolean ? e.Value.Value.BooleanValue.ToString() :
                        e.Value.Value.Type == DataType.Date || e.Value.Value.Type == DataType.DateTime ? e.Value.Value.DateTimeValue.Format(null) :
                        e.Value.Value.Value.ToString(),
                    Exception = e.Value.Exception,
                    Unique = (int)e.Value.Unique,
                    Notification = e.Value.Notification,
                    NotificationEmail = e.Value.Notification ? e.Value.NotificationEmail : null,
                    NotificationNote = e.Value.Notification ? e.Value.NotificationNote : null,
                    Notified = e.Value.Notification && e.Value.Notified,
                    NotificationDate =
                        !e.Value.Notification ? default(DateTimeOffset?) :
                        e.Value.Value.Type == DataType.Boolean ? DateTimeOffset.UtcNow :
                        e.Value.Value.DateTimeValue
                }).ToList();
            cacheObject.Instance.Expressions.Values.Where(e => e.Notification).ToList().ForEach(e => e.Notified = true);
            cacheObject.NeedToCache = true;
            return Result;
        }

        private async Task UpdateParentInstance(InstanceCacheObject cacheObject, IDefaultIdentity identity, bool Refresh)
        {
            if (cacheObject.Instance.ParentInstanceCode != null)
                try
                {
                    //tries to update the parent instance link
                    var parentCacheObject = await HandleEventAsync(identity, new InstanceEventRequest
                    {
                        Code = cacheObject.Instance.ParentInstanceCode,
                        ItemId = cacheObject.Instance.ParentInstanceLinkID,
                        Instance = cacheObject.ID,
                        Type = Refresh ? EventTypes.RefreshAttachedInstance : EventTypes.AttachInstance
                    });
                }
                catch { }
        }

        private async Task<InstanceCacheObject> UpdateDataBaseAsync(InstanceCacheObject cacheObject, IDefaultIdentity identity, bool close, bool complete, bool alreadyLocked, string ModifiedBy, DateTimeOffset? ModifiedOn)
        {
            LockObject lockObject = null;

            try
            {
                if (cacheObject.InstanceState != InstanceState.Progress)
                    throw new InstanceException("Instance state not valid");

                if (!alreadyLocked)
                {
                    lockObject = await _instanceCache.LockAsync(cacheObject.Code);
                    cacheObject = await _instanceCache.GetAsync(cacheObject.Code, true);
                }

                if (complete)
                {
                    var InvalidItems = cacheObject.Instance.Items.Values.Where(i => !i.Removed && !i.IteratorModelItem && i is ICheckListValidityElement checkListValidityElement && !checkListValidityElement.Valid);
                    if (InvalidItems.Any())
                        throw new InstanceException($"The following items are not valid: {string.Join(", ", InvalidItems.Select(i => i.ToString()))}");

                    var InvalidPage = cacheObject.Instance.Pages.FirstOrDefault(p => !p.Valid);
                    if (InvalidPage != null)
                        throw new InstanceException($"The page {InvalidPage.Description} is not valid{(InvalidPage.ValidityMessage == null ? string.Empty : $": {InvalidPage.ValidityMessage}")}");
                }

                var row = await _instancesRepository.GetAsync(cacheObject.CompanyId, cacheObject.Code);
                if (row == null)
                {
                    await _instanceCache.RemoveAsync(cacheObject.Code);
                    throw new InstanceException("Instance not found");
                }

                if (!cacheObject.Instance.CheckVersion(2, true)) //version 2 introduces persistency
                    cacheObject.Instance.UpdateAll(cacheObject);

                cacheObject.NeedToSave = false;
                row.Data = JsonConvert.SerializeObject(cacheObject.Instance, _converters);
                row.Description = cacheObject.Instance.DescriptionFormula.Value;
                row.FileName =
                    !cacheObject.Instance.FTPExport ? null :
                    !string.IsNullOrWhiteSpace(cacheObject.Instance.FileNameFormula.Value) ? cacheObject.Instance.FileNameFormula.Value :
                    $"Instance{cacheObject.InstanceCode}";
                row.ModifiedOn = ModifiedOn;
                row.ModifiedBy = ModifiedBy;

                await UpdateJSONFieldsAsync(cacheObject);

                _instancesRepository.SaveChildren(row.ID, CreateTags(cacheObject), CreateExpressionValues(cacheObject));

                bool ExportFTP = false;

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

                        if (!row.PrototypeVersions.VersionState.In(VersionState.Draft, VersionState.InSimulation) && !string.IsNullOrEmpty(row.FileName))
                            ExportFTP = true;
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


                if (ExportFTP)
                    await _queueService.SendObjectAsync(QueueName.InstancesFTP, new InstancesFTPMessage
                    {
                        Company = identity.CompanyId,
                        ID = row.ID,
                        Path = row.FileName,
                        Code = cacheObject.InstanceCode
                    });

                await UpdateParentInstance(cacheObject, identity, true);

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

                    await UpdateJSONFieldsAsync(cacheObject);
                }

                await _instancesRepository.SaveChangesAsync();
                await UpdateParentInstance(cacheObject, identity, true);
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
                response.Version = version;
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
                ID = instance.ID,
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
                    && (identity.HasPermission("Instances", function, Department) || identity.HasPermission("Instances", function, Department, PrototypeType))
                    && (Tag == null || identity.HasPermission("SignatureLevel", null, Department, tag: Tag) || identity.HasPermission("SignatureLevel", null, Department, PrototypeType, tag: Tag));
        }

        #endregion

        #region actions

        private async Task<DataSource> GetExternalSource(InstanceCacheObject cacheObject, string externalSourceCode)
        {
            var response = await _client.PostAsync("DataSources/Data/GetDataSource", new { code = cacheObject.Instance.ExternalSources[externalSourceCode].DataSource });
            var responseData = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception(responseData);

            var DataSource = JsonConvert.DeserializeObject<DataSource>(responseData);

            return DataSource;
        }

        private List<ExternalSourceResponse.Parameter> GetExternalSourceParameters(InstanceCacheObject cacheObject, ExternalSource externalSource, DataSource dataSource, Item item)
        {
            return dataSource.Parameters.Select(p =>
            {
                if (externalSource.Parameters == null || !externalSource.Parameters.TryGetValue(p.Name, out var InstanceParameter)) InstanceParameter = null;

                string Value = null;
                string Error = null;
                string Binding = null;
                if (InstanceParameter?.ExpressionCode != null)
                {
                    Binding = $"expression {InstanceParameter.ExpressionCode}";
                    if (cacheObject.Instance.Expressions.TryGetValue(InstanceParameter.ExpressionCode, out var Expression))
                        Value = Expression.Value?.Value?.ToString();
                    else
                        Error = $"Expression {InstanceParameter.ExpressionCode} not found";
                }
                else if (InstanceParameter?.ElementCode != null)
                {
                    Binding = $"field {InstanceParameter.ElementCode}";
                    if (!item.GetBranchElements().TryGetValue(InstanceParameter.ElementCode, out var Element))
                        Error = $"Element {InstanceParameter.ElementCode} not found";
                    else if (!(Element is Field Field))
                        Error = $"Element {InstanceParameter.ElementCode} is not a Field";
                    else
                        Value = Field.Value;
                }

                if (Value != null && !DataType.FromName(p.ParameterType).TryParse(Value, out _))
                {
                    Error = $"The value {Value} of the {Binding} is not a valid {p.ParameterType}";
                    Value = null;
                }

                return new ExternalSourceResponse.Parameter
                {
                    Name = p.Name,
                    Label = p.Label,
                    ParameterType = p.ParameterType,
                    Hidden = InstanceParameter?.Hidden ?? false,
                    ReadOnly = InstanceParameter?.ReadOnly ?? false,
                    Value = Value,
                    Error = Error
                };
            }).ToList();
        }

        public async Task<InstanceLinkParametersResponse> GetInstanceLinkParameters(string code, IDefaultIdentity identity, int itemId)
        {
            try
            {
                var cacheObject = await GetCacheObjectForEditAsync(identity, code, true);
                if (cacheObject.InstanceState != InstanceState.Draft && cacheObject.InstanceState != InstanceState.Progress)
                    throw new InstanceException("Instance state not valid");
                if (!HasPermission(identity, "Edit", cacheObject.VersionState, cacheObject.Department, cacheObject.PrototypeType))
                    throw new InstanceException("Access denied");
                if (!cacheObject.Instance.Items.TryGetValue(itemId, out var item) || !(item is InstanceLink instanceLink))
                    throw new InstanceException($"InstanceLink {itemId} not found");

                await _instanceCache.TouchAsync(code);

                return new InstanceLinkParametersResponse
                {
                    Parameters = ((InstanceLink)instanceLink.FindIteratorModelItem()).Parameters?.Select(p =>
                    {
                        string Value = null;
                        string Error = null;
                        if (p.ExpressionCode != null)
                        {
                            if (cacheObject.Instance.Expressions.TryGetValue(p.ExpressionCode, out var Expression))
                            {
                                var TypedValue = Expression.Value;
                                Value =
                                    TypedValue?.Type == DataType.DateTime ? TypedValue?.DateTimeValue.Format(cacheObject.Instance.TimeZone, DateTimeOffsetHelper.DateTimeFormats[0]) :
                                    TypedValue?.Type == DataType.Date ? TypedValue?.DateTimeValue.Format(cacheObject.Instance.TimeZone, DateTimeOffsetHelper.DateFormat) :
                                    TypedValue?.Value?.ToString();
                            }
                            else
                                Error = $"Expression {p.ExpressionCode} not found";
                        }
                        else if (p?.ElementCode != null)
                        {
                            if (!instanceLink.GetBranchElements().TryGetValue(p.ElementCode, out var Element))
                                Error = $"Element {p.ElementCode} not found";
                            else if (!(Element is Field Field))
                                Error = $"Element {p.ElementCode} is not a Field";
                            else
                                Value = Field.Value;
                        }
                        else
                            Value = p.Value;

                        return new InstanceLinkParametersResponse.Parameter
                        {
                            Name = p.Name,
                            Value = Value,
                            Error = Error
                        };
                    }).ToList()
                };
            }
            catch (InstanceException)
            {
                throw;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Unable to retrieve the instance link parameters");
                throw new InstanceException("Unable to retrieve the instance link parameters", e);
            }
        }

        public async Task<ExternalSourceResponse> GetExternalSource(string code, IDefaultIdentity identity, string externalSourceCode, int itemId)
        {
            try
            {
                var cacheObject = await GetCacheObjectForEditAsync(identity, code, true);
                if (cacheObject.InstanceState != InstanceState.Draft && cacheObject.InstanceState != InstanceState.Progress)
                    throw new InstanceException("Instance state not valid");
                if (!HasPermission(identity, "Edit", cacheObject.VersionState, cacheObject.Department, cacheObject.PrototypeType))
                    throw new InstanceException("Access denied");
                if (!cacheObject.Instance.ExternalSources.TryGetValue(externalSourceCode, out var externalSource))
                    throw new InstanceException("Invalid external source");
                if (!cacheObject.Instance.Items.TryGetValue(itemId, out var item))
                    throw new InstanceException($"Item {itemId} not found");

                await _instanceCache.TouchAsync(code);

                var dataSource = await GetExternalSource(cacheObject, externalSourceCode);
                var parameters = GetExternalSourceParameters(cacheObject, externalSource, dataSource, (Item)item);

                return new ExternalSourceResponse
                {
                    Columns = dataSource.Columns,
                    Parameters = parameters
                };
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

        public async Task<string> GetExternalSourceRows(string code, IDefaultIdentity identity, string externalSourceCode, int itemId, Dictionary<string, string> clientParameters, int page)
        {
            try
            {
                var cacheObject = await GetCacheObjectForEditAsync(identity, code, true);
                if (cacheObject.InstanceState != InstanceState.Draft && cacheObject.InstanceState != InstanceState.Progress)
                    throw new InstanceException("Instance state not valid");
                if (!HasPermission(identity, "Edit", cacheObject.VersionState, cacheObject.Department, cacheObject.PrototypeType))
                    throw new InstanceException("Access denied");
                if (!cacheObject.Instance.ExternalSources.TryGetValue(externalSourceCode, out var externalSource))
                    throw new InstanceException("Invalid external source");
                if (!cacheObject.Instance.Items.TryGetValue(itemId, out var item))
                    throw new InstanceException($"Item {itemId} not found");

                await _instanceCache.TouchAsync(code);

                var dataSource = await GetExternalSource(cacheObject, externalSourceCode);
                var dataSourceParameters = GetExternalSourceParameters(cacheObject, externalSource, dataSource, (Item)item);
                var parameters = dataSourceParameters.ToDictionary(k => k.Name, v => !v.ReadOnly && !v.Hidden && clientParameters.TryGetValue(v.Name, out var clientValue) ? clientValue : v.Error == null ? v.Value : null);

                var token = identity.Code + ":" + Guid.NewGuid();
                await _redisCache.SetAsync($"Token:{token}", new DataRequest
                {
                    code = cacheObject.Instance.ExternalSources[externalSourceCode].DataSource,
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
                Logger.Error(e, "Unable to retrieve the external source rows");
                throw new InstanceException("Unable to retrieve the external source rows", e);
            }
        }

        public async Task<List<InstanceRelation>> GetParentInstances(string code, IDefaultIdentity identity)
        {
            var cacheObject = await GetCacheObjectAsync(identity.CompanyId, code, true, false);
            if (!HasPermission(identity, "View", cacheObject.VersionState, cacheObject.Department, cacheObject.PrototypeType))
                throw new InstanceException("Access denied");

            var list = new List<InstanceRelation>();
            if (cacheObject.ID.HasValue)
                list = await _instancesRepository.GetParentInstances(cacheObject.ID.Value);
            if (cacheObject.Instance.ParentInstanceCode != null && cacheObject.Instance.ParentInstanceLinkID.HasValue && !list.Any(r => r.Code == cacheObject.Instance.ParentInstanceCode))
                try
                {
                    var parentCacheObject = await GetCacheObjectAsync(identity.CompanyId, cacheObject.Instance.ParentInstanceCode, true, false);
                    list.Add(new InstanceRelation
                    {
                        Prototype = parentCacheObject.PrototypeName,
                        Version = parentCacheObject.Version,
                        Year = parentCacheObject.Year,
                        Number = parentCacheObject.Number,
                        VersionState = parentCacheObject.VersionState,
                        Description = parentCacheObject.Instance.Description,
                        ID = parentCacheObject.ID,
                        Item = cacheObject.Instance.ParentInstanceLinkID.Value,
                        Code = cacheObject.Instance.ParentInstanceCode,
                        RelationType = InstanceRelation.RelationTypes.Parent
                    });
                }
                catch { } //the child ignores if the parent instance has been deleted
            list.ForEach(r => r.InstanceCode = Instance.GetCode(r.Prototype, r.Version, r.VersionState, r.Year, r.Number));
            return list;
        }

        private async Task DataSourceCalls(InstanceCacheObject cacheObject, IDefaultIdentity identity, Signature signature)
        {
            foreach (var Element in signature.GetBranchElements().Values)
                if (Element is Group Group && Group.SignatureCode == signature.Code && Group.DumpExternalSourceName != null)
                    try
                    {
                        var Iterations = Group.Iterations.Where(i => !i.Removed).Select(i => i.Items.Where(i2 => i2.Code != null && i2 is Field).Select(i2 => (Item)i2));
                        if (Iterations.Any() && Iterations.First().Any())
                        {
                            var Names = Iterations.First().Select(i2 => i2.Code).ToList();
                            var Records = Iterations.Select(i => i.Select(i2 => ((Field)i2).Value).ToList()).ToList();

                            var token = identity.Code + ":" + Guid.NewGuid();
                            await _redisCache.SetAsync($"Token:{token}", new ExportRequest
                            {
                                Code = cacheObject.Instance.ExternalSources[Group.DumpExternalSourceName].DataSource,
                                Names = Names,
                                Records = Records,
                                DecimalSeparator = await _settingService.DecimalSeparatorAsync(identity.CompanyId),
                                TimeZone = (await _commonRepository.GetCompanyAsync(identity.CompanyId)).TimeZone
                            }, "S2S", new TimeSpan(0, 1, 0));
                            var response = await _client.PostAsync("DataSources/Data/Export", new TokenRequest { token = token });
                            var responseData = await response.Content.ReadAsStringAsync();
                            if (response.StatusCode != HttpStatusCode.OK)
                                throw new Exception(responseData);
                        }
                    }
                    catch (Exception Exception)
                    {
                        Logger.Error(Exception, $"Error while exporting the content of {Element}");
                        throw new InstanceException($"Error while exporting the content of {Element}", Exception);
                    }

            if (signature.CommandExternalSourceName != null)
                try
                {
                    if (!cacheObject.Instance.ExternalSources.TryGetValue(signature.CommandExternalSourceName, out var externalSource))
                        throw new InstanceException("Invalid external source");

                    var dataSource = await GetExternalSource(cacheObject, signature.CommandExternalSourceName);
                    var dataSourceParameters = GetExternalSourceParameters(cacheObject, externalSource, dataSource, signature);
                    var parameters = dataSourceParameters.ToDictionary(k => k.Name, v => v.Value);

                    var token = identity.Code + ":" + Guid.NewGuid();
                    await _redisCache.SetAsync($"Token:{token}", new DataRequest
                    {
                        code = cacheObject.Instance.ExternalSources[signature.CommandExternalSourceName].DataSource,
                        parameters = parameters,
                        page = 0,
                        decimalSeparator = await _settingService.DecimalSeparatorAsync(identity.CompanyId),
                        timeZone = (await _commonRepository.GetCompanyAsync(identity.CompanyId)).TimeZone
                    }, "S2S", new TimeSpan(0, 1, 0));
                    var response = await _client.PostAsync("DataSources/Data/GetDataSourceRows", new TokenRequest { token = token });
                    var responseData = await response.Content.ReadAsStringAsync();
                    if (response.StatusCode != HttpStatusCode.OK)
                        throw new Exception(responseData);
                }
                catch (Exception Exception)
                {
                    Logger.Error(Exception, $"Error while invoking command {signature.CommandExternalSourceName} the content of {signature.ToString()}");
                    throw new InstanceException($"Error while invoking command {signature.CommandExternalSourceName} the content of {signature.ToString()}", Exception);
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

                return (await _instancesRepository.GetInstanceEditors(identity.CompanyId, cacheObject.Department, cacheObject.PrototypeType, cacheObject.VersionState)).Select(u => new InstanceEventRequest.Email
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

        public async Task ClosePendingInstances(CancellationToken cancellationToken = new CancellationToken())
        {
            var score = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - _configurationService.InstanceCloseTimeout * 60;
            var codes = await _instanceCache.GetInstancesInRangeAsync(stop: score);

            foreach (var code in codes)
                try
                {
                    var cacheObject = await _instanceCache.GetAsync(code, false);
                    if (cacheObject == null)
                        continue;

                    if (cacheObject.NeedToSave)
                        switch (cacheObject.InstanceState)
                        {
                            case InstanceState.Draft:
                                await _instanceCache.RemoveAsync(cacheObject.Code);
                                break;
                            case InstanceState.Progress:
                                await UpdateDataBaseAsync(cacheObject, null, true, false, false, cacheObject.ModifiedBy, cacheObject.ModifiedOn);
                                break;
                            default:
                                throw new InstanceException($"Invalid state: {cacheObject.InstanceState}");
                        }
                    else
                        await _instanceCache.RemoveAsync(code);
                }
                catch (Exception exception)
                {
                    await _emailService.LogErrorAsync(exception, $"Failed to clear pending instance with code: {code}");
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

            return (await Task.WhenAll(Instance.InstancesHistory.OrderBy(h => h.ID).Select(async h => new HistoryRow
            {
                ID = h.ID,
                Date = h.Date,
                User = $"{h.User} - {_commonRepository.GetUser(identity.CompanyId, h.User)?.Description ?? "(unknown user)"}",
                State = h.WorkFlowApplications == null ? h.State : h.WorkFlowApplications.FinalState,
                ProposedState = h.WorkFlowApplications == null ? null : InstanceState.Confirmed,
                Approved = h.WorkFlowApplications?.Approved,
                Note = h.Note,
                Steps = h.WorkFlowApplications == null ? null : (await Task.WhenAll(h.WorkFlowApplications?.WorkFlowApplicationSteps.OrderBy(s => s.Step).ThenBy(s => s.ID).Select(async s => new HistoryStep
                {
                    ID = s.ID,
                    Step = s.Step + 1,
                    Date = s.Date,
                    User = s.User == null ? string.Empty : $"{s.User} - {s.UserDescription}",
                    Level = $"({string.Join(", ", s.WorkFlowApplicationStepTags.Select(t => t.Tag))})",
                    Reason = s.Reason,
                    Comment = s.Comment,
                    Approved = s.Approved,
                    AppointedUser = s.User == null ? string.Join(", ", (await _commonRepository.GetAppointedUsersAsync(identity.CompanyId, "Instances", "Confirm", Instance.Prototypes.Department, Instance.Prototypes.PrototypeType, "InstanceWorkFlowLevel", s.WorkFlowApplicationStepTags.Select(t => t.Tag))).Select(u => u.Code)) : null
                }))).ToList()
            }))).ToList();
        }

        public async Task InvalidateCacheAsync(DefaultIdentity identity, InvalidateCacheRequest request)
        {
            foreach (var code in request.Instances)
                try
                {
                    var cacheObject = await _instanceCache.GetAsync(code, false);
                    if (cacheObject != null)
                    {
                        if (cacheObject.Prototype != request.Prototype || cacheObject.Version != request.Version)
                            Logger.Warn($"Incongruent instance prototype and version: Received {request.Prototype}-{request.Version}, expected: {cacheObject.Prototype}-{cacheObject.Version}");

                        if (cacheObject.Year != 0)
                            Logger.Warn($"Invalid year for instance {cacheObject.Code}: {cacheObject.Year?.ToString() ?? "?"}");

                        var prototypeVersion = await _instancesRepository.GetPrototypeVersionAsync(request.Prototype, request.Version);
                        if (
                            !identity.HasPermission("Prototypes", "Preview", prototypeVersion.Prototypes.Department) &&
                            !identity.HasPermission("Prototypes", "Preview", prototypeVersion.Prototypes.Department, prototypeVersion.Prototypes.PrototypeType) &&
                            !identity.HasPermission("Prototypes", "Edit", prototypeVersion.Prototypes.Department) &&
                            !identity.HasPermission("Prototypes", "Edit", prototypeVersion.Prototypes.Department, prototypeVersion.Prototypes.PrototypeType)
                        )
                            throw new UnauthorizedAccessException();

                        if (request.OnlyDeletion || !cacheObject.NeedToSave)
                            await _instanceCache.RemoveAsync(cacheObject.Code);
                        else
                            await UpdateDataBaseAsync(cacheObject, identity, true, false, false, cacheObject.ModifiedBy, cacheObject.ModifiedOn);
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

        public async Task SendNotificationsAsync(CancellationToken cancellationToken)
        {
            var Notifications = await _instancesRepository.GetNotifications();
            foreach (var Notification in Notifications)
                try
                {
                    if (cancellationToken.IsCancellationRequested) return;

                    var InstanceCode = Instance.GetCode(Notification.Instances.PrototypeName, Notification.Instances.Version, Notification.Instances.PrototypeVersions.VersionState, Notification.Instances.Year, Notification.Instances.Number);
                    var Emails = Notification.NotificationEmail.Split(';');
                    foreach (var Email in Emails)
                        await _queueService.SendObjectAsync(QueueName.Email, new EmailMessage(
                            Notification.Instances.Company,
                            "Notification",
                            await _settingService.NotificationMessageAsync(Notification.Instances.Company),
                            Email.Trim(),
                            new List<Tuple<string, string>>
                            {
                                new Tuple<string, string>("Code", Notification.Code),
                                new Tuple<string, string>("Description", Notification.Description),
                                new Tuple<string, string>("Note", Notification.NotificationNote),
                                new Tuple<string, string>("Value", Notification.Value),
                                new Tuple<string, string>("Link", $"<a href='{Instance.GetUrl(_configurationService, Notification.Instances.Code)}' target='_blank'>{InstanceCode}</a>")
                            }
                        )
                        {
                            TrailMessage = new EmailTrailMessage(
                                "(system)",
                                InstanceCode,
                                Notification.Instance,
                                null
                            )
                        }, cancellationToken);

                    Notification.Notified = true;

                    await _instancesRepository.SaveChangesAsync();
                }
                catch (Exception Exception)
                {
                    Logger.Error(Exception);
                    await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
                    {
                        Company = Notification.Instances.Company,
                        Action = TrailAction.JobError,
                        User = "(system)",
                        Date = DateTimeOffset.UtcNow,
                        CheckListCode = Instance.GetCode(Notification.Instances.PrototypeName, Notification.Instances.Version, Notification.Instances.PrototypeVersions.VersionState, Notification.Instances.Year, Notification.Instances.Number),
                        CheckListID = Notification.Instances.ID,
                        Information = JsonConvert.SerializeObject(new
                        {
                            Job = "SendNotifications",
                            Exception = Exception.Message
                        })
                    }, cancellationToken);
                    await _emailService.LogErrorAsync(Exception, "Error sending notification");
                }
        }


        #endregion

        #region events

        public async Task<InstanceCacheObject> GetInstanceAsync(IDefaultIdentity identity, InstanceCacheObject cacheObject)
        {
            LockObject lockObject = null;
            Page page;
            try
            {
                lockObject = await _instanceCache.LockAsync(cacheObject.Code);
                cacheObject = await GetCacheObjectForEditAsync(identity, cacheObject.Code, true);
                if (cacheObject.InstanceState == InstanceState.Progress && cacheObject.Instance.Pages.Any() && (page = (Page)cacheObject.Instance.Pages.First()).Lock == null && HasPermission(identity, "Edit", cacheObject.VersionState, cacheObject.Department, cacheObject.PrototypeType))
                {
                    UnlockPages(cacheObject.Instance, identity.Code);
                    page.Lock = new Lock(identity.Code);
                    cacheObject.NeedToCache = true;
                    await SaveCacheObjectAsync(cacheObject);
                }
            }
            finally
            {
                lockObject?.Dispose();
            }

            await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
            {
                Company = identity.CompanyId,
                User = identity.Code,
                Action = TrailAction.InstanceOpened,
                Date = DateTimeOffset.UtcNow,
                CheckListCode = cacheObject.InstanceCode,
                CheckListID = cacheObject.ID
            });

            return cacheObject;
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
                    stopWatch = Stopwatch.StartNew();
                    lockObject = await _instanceCache.LockAsync(request.Code);
                    stopWatch.Stop();
                    lockTime = stopWatch.Elapsed.TotalMilliseconds;
                }

                stopWatch = Stopwatch.StartNew();
                cacheObject = await GetCacheObjectForEditAsync(identity, request.Code, true);
                stopWatch.Stop();
                getTime = stopWatch.Elapsed.TotalMilliseconds;

                //A signer may sign also if the page is locked by the main user (the lock is for concurrency purpose, not for security), NB: SignatureCheck is before the choice of signer
                var lockUser = request.Type == "Signature" ? request.Username : identity.Code;
                cacheObject.Instance.UpdateReadOnly(lockUser, cacheObject.InstanceState, request.Type == EventTypes.AttachInstance || request.Type == EventTypes.RefreshAttachedInstance);

                stopWatch = Stopwatch.StartNew();

                DependencyNode AffectedElement = null;
                IEnumerable<DependencyNode> AffectedElements = null;
                DependencyRelation.Properties? Property = null;
                switch (request.Type)
                {
                    case EventTypes.PageChange:
                        HandlePageChangeAsync(cacheObject, identity.Code, request.ItemId);
                        Property = DependencyRelation.Properties.ReadOnly;
                        break;
                    case EventTypes.ValueChange:
                        AffectedElement = await HandleValueChangeAsync(cacheObject, identity.Code, request.ItemId, request.Value);
                        Property = DependencyRelation.Properties.Value;
                        break;
                    case EventTypes.ValueCopy:
                        AffectedElement = await HandleValueCopyAsync(cacheObject, identity.Code, request.ItemId);
                        Property = DependencyRelation.Properties.Value;
                        break;
                    case EventTypes.ExternalSourceRowChange:
                        AffectedElements = await HandleExternalSourceRowChange(cacheObject, identity, request.ItemId, request.Row);
                        Property = AffectedElements == null ? DependencyRelation.Properties.ReadOnly : DependencyRelation.Properties.Value;
                        break;
                    case EventTypes.Import:
                        await HandleImport(cacheObject, identity, request.ItemId, request.Value, request.Columns, request.Rows);
                        Property = DependencyRelation.Properties.ReadOnly;
                        break;
                    case EventTypes.Add:
                        AffectedElement = await HandleAddAsync(cacheObject, identity.Code, request.ItemId);
                        Property = DependencyRelation.Properties.Iterations;
                        break;
                    case EventTypes.Remove:
                        AffectedElement = await HandleRemoveAsync(cacheObject, identity.Code, request.ItemId);
                        Property = DependencyRelation.Properties.Iterations;
                        break;
                    case EventTypes.Up:
                        AffectedElement = await HandleMoveAsync(cacheObject, identity.Code, request.ItemId, -1);
                        Property = DependencyRelation.Properties.Iterations;
                        break;
                    case EventTypes.Down:
                        AffectedElement = await HandleMoveAsync(cacheObject, identity.Code, request.ItemId, 1);
                        Property = DependencyRelation.Properties.Iterations;
                        break;
                    case EventTypes.SignatureCheck:
                        HandleSignatureCheckAsync(cacheObject, request.Username, request.ItemId);
                        break;
                    case EventTypes.Signature:
                        AffectedElement = await HandleSignatureAsync(cacheObject, identity, request);
                        Property = DependencyRelation.Properties.Signature;
                        break;
                    case EventTypes.SignatureCancelation:
                        AffectedElement = await HandleSignatureCancellationAsync(cacheObject, identity, request);
                        Property = DependencyRelation.Properties.Signature;
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
                        AffectedElement = HandleSkipCountDown(cacheObject, request.Username, request.ItemId);
                        Property = DependencyRelation.Properties.CountDown;
                        break;
                    case EventTypes.Refresh:
                        break;
                    case EventTypes.Quit:
                        await HandleQuitAsync(cacheObject, identity);
                        Property = DependencyRelation.Properties.ReadOnly;
                        break;
                    case EventTypes.UploadAttachment:
                        AffectedElement = await HandleUploadAttachment(cacheObject, identity, request.ItemId, request.Attachment);
                        Property = DependencyRelation.Properties.Validity;
                        break;
                    case EventTypes.AttachInstance:
                        AffectedElement = await HandleAttachInstance(cacheObject, identity, request.ItemId, request.Instance);
                        Property = DependencyRelation.Properties.Value;
                        break;
                    case EventTypes.RefreshAttachedInstance:
                        AffectedElement = await HandleRefreshAttachedInstance(cacheObject, identity, request.ItemId, request.Instance);
                        Property = DependencyRelation.Properties.Value;
                        break;
                    default:
                        throw new InstanceException($"Event type {request.Type} not handled.");
                }

                var Roots = new HashSet<Tuple<DependencyNode, DependencyRelation.Properties>>();
                if (Property == DependencyRelation.Properties.Iterations)
                {
                    var Iteration = (Iteration)AffectedElement;
                    var IterationChildren = new List<DependencyNode>();
                    Iteration.GetAllChildren(IterationChildren);
                    IterationChildren = IterationChildren.Where(x => x != null).ToList();

                    if (IterationChildren.Any())
                        foreach (var Child in IterationChildren.Cast<Element>())
                            Child.MarkAllAsChanged(Roots);

                    AffectedElement = (Group)Iteration.Group;
                }

                if (AffectedElements == null && AffectedElement != null)
                    AffectedElements = new List<DependencyNode> { AffectedElement };

                if (AffectedElements != null)
                {
                    foreach (var Element in AffectedElements)
                        Element.MarkNodeAsChanged(cacheObject.Instance, Roots, Property.Value);
                    DependencyNode.UpdateNodes(cacheObject.Instance, Roots);
                    cacheObject.Instance.UpdateReadOnly(lockUser ?? identity.Code, cacheObject.InstanceState, false);
                }
                else if (Property == DependencyRelation.Properties.ReadOnly)
                    cacheObject.Instance.UpdateReadOnly(lockUser, cacheObject.InstanceState, false);

                stopWatch.Stop();
                handleTime = stopWatch.Elapsed.TotalMilliseconds;

                await SaveCacheObjectAsync(cacheObject);
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

        private async Task<Field> HandleValueChangeAsync(InstanceCacheObject cacheObject, string userCode, Field field, string value, int? itemId, string Action)
        {
            if ((value ?? string.Empty) == (field.Value ?? string.Empty)) return null;

            string
                oldFormattedValue = field.Value,
                formattedValue = value,
                loggedValue = value;

            field.ConversionError = false;
            if (string.IsNullOrEmpty(value))
                formattedValue = loggedValue = null;
            else if (field.ValueType == DataType.DateTime)
            {
                string TimeZone = cacheObject.Instance.TimeZone;

                if (field.LocalTimeZone)
                {
                    DateTimeOffsetHelper.TryParse(oldFormattedValue, TimeZone, out _, out _, out var dateTimeOffset);
                    oldFormattedValue = dateTimeOffset?.Format(TimeZone) ?? string.Empty;
                    DateTimeOffsetHelper.TryParse(formattedValue, TimeZone, out _, out _, out dateTimeOffset);
                    loggedValue = dateTimeOffset?.Format(TimeZone) ?? string.Empty;
                    if (!dateTimeOffset.HasValue) formattedValue = string.Empty;
                }
                else
                {
                    if (DateTimeOffset.TryParseExact(oldFormattedValue, DateTimeOffsetHelper.DateTimeFormats, null, System.Globalization.DateTimeStyles.AssumeUniversal, out var Date))
                        oldFormattedValue = Date.Format(null);
                    else
                        oldFormattedValue = string.Empty;

                    if (DateTimeOffset.TryParseExact(formattedValue, DateTimeOffsetHelper.DateTimeFormats, null, System.Globalization.DateTimeStyles.AssumeUniversal, out Date))
                        loggedValue = Date.Format(null);
                    else
                        formattedValue = loggedValue = string.Empty;
                }
            }
            else if (field.ValueType == DataType.Number)
            {
                DecimalHelper.Parse(value, field.Precision, field.ScientificNotation, cacheObject.Instance.DecimalSeparator, out _, out formattedValue);
                if (formattedValue == null)
                {
                    field.ConversionError = true;
                    formattedValue = value;
                }

                loggedValue = formattedValue;
            }
            else if (field.ValueType == DataType.Time)
            {
                if (DataType.Time.TryParse(value, out var parsedValue))
                    formattedValue = loggedValue = ((TimeSpan)parsedValue).ToString("hh\\:mm");
                else
                {
                    field.ConversionError = true;
                    formattedValue = loggedValue = string.Empty;
                }
            }
            else if (!field.ValueType.TryParse(value, out var parsedValue))
                field.ConversionError = true;

            field.ModifiedBy = userCode;
            field.ModifiedOn = DateTimeOffset.UtcNow;
            field.History.Add(new FieldRecord
            {
                DateTime = field.ModifiedOn.Value,
                User = field.ModifiedBy,
                Value = loggedValue,
                TypedValue = value
            });

            await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
            {
                Company = cacheObject.CompanyId,
                User = userCode,
                Action = Action,
                Date = DateTimeOffset.UtcNow,
                CheckListCode = cacheObject.InstanceCode,
                CheckListID = cacheObject.ID,
                Information = JsonConvert.SerializeObject(new
                {
                    ID = itemId,
                    OldValue = oldFormattedValue,
                    NewValue = loggedValue
                })
            });
            field.Value = formattedValue;

            TrackChanges(cacheObject, userCode);

            return field;
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

            return await HandleValueChangeAsync(cacheObject, userCode, field, value, itemId, TrailAction.InstanceValueChanged);
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

            return await HandleValueChangeAsync(cacheObject, userCode, field, value, itemId, TrailAction.InstanceValueCopied);
        }

        private async Task<Iteration> HandleAddAsync(InstanceCacheObject cacheObject, string userCode, int? itemId)
        {
            if (!itemId.HasValue)
                throw new InstanceException("Item id not sent");

            var group = cacheObject.Instance.Items.ContainsKey(itemId.Value) ? cacheObject.Instance.Items[itemId.Value] as Group : null;
            if (group == null)
                throw new InstanceException("Group not found");

            if (!group.Iterator || !group.AddIterationButton)
                throw new InstanceException("Invalid operation");

            Iteration Iteration = null;
            if (!group.ReadOnly)
            {
                Iteration = group.AddIteration();
                group.SetupIterationsIndexes();
                group.ModifiedBy = userCode;
                group.ModifiedOn = DateTimeOffset.UtcNow;
                await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
                {
                    Company = cacheObject.CompanyId,
                    User = userCode,
                    Action = TrailAction.InstanceIterationAdded,
                    Date = DateTimeOffset.UtcNow,
                    CheckListCode = cacheObject.InstanceCode,
                    CheckListID = cacheObject.ID,
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

            if (!iteration.Group.Iterator || !((Group)iteration.Group).RemoveIterationButton)
                throw new InstanceException("Invalid operation");

            if (iteration.ReadOnly) return null;

            var group = (Group)iteration.Group;
            group.Remove(iteration);
            group.SetupIterationsIndexes();
            group.ModifiedBy = userCode;
            group.ModifiedOn = DateTimeOffset.UtcNow;
            await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
            {
                Company = cacheObject.CompanyId,
                User = userCode,
                Action = TrailAction.InstanceIterationDeleted,
                Date = DateTimeOffset.UtcNow,
                CheckListCode = cacheObject.InstanceCode,
                CheckListID = cacheObject.ID,
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

            if (!iteration.Group.Iterator || !((Group)iteration.Group).MoveIterationButton)
                throw new InstanceException("Invalid operation");

            if (iteration.ReadOnly) return null;

            var group = (Group)iteration.Group;
            group.Move(iteration, direction);
            group.SetupIterationsIndexes();
            group.ModifiedBy = userCode;
            group.ModifiedOn = DateTimeOffset.UtcNow;
            await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
            {
                Company = cacheObject.CompanyId,
                User = userCode,
                Action = direction == 1 ? TrailAction.InstanceIterationMovedDown : TrailAction.InstanceIterationMovedUp,
                Date = DateTimeOffset.UtcNow,
                CheckListCode = cacheObject.InstanceCode,
                CheckListID = cacheObject.ID,
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

            signature.TrySign(userCode, DateTimeOffset.Now);
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

                var Now = DateTimeOffset.UtcNow;
                signature.TrySign(identity.Code, Now);

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

                cacheObject.ModifiedBy = identity.Code;
                cacheObject.ModifiedOn = Now;
                signature.Sign(identity.Code, identity.Description, Now);
                await DataSourceCalls(cacheObject, identity, signature);

                if (cacheObject.InstanceState == InstanceState.Progress)
                    await UpdateDataBaseAsync(cacheObject, identity, false, false, true, cacheObject.ModifiedBy, cacheObject.ModifiedOn);
                TrackChanges(cacheObject, request.Username);

                await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
                {
                    Company = identity.CompanyId,
                    User = identity.Code,
                    Action = TrailAction.InstanceSignatureIssued,
                    Date = DateTimeOffset.UtcNow,
                    CheckListCode = cacheObject.InstanceCode,
                    CheckListID = cacheObject.ID,
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
                    CheckListCode = cacheObject.InstanceCode,
                    CheckListID = cacheObject.ID,
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
                    throw new InstanceException($"The signature can be canceled by an user with function Unsign{(string.IsNullOrWhiteSpace(signature.SignatureLevel) ? string.Empty : $" and level {signature.SignatureLevel}")}");

                signature.Unsign(identity.Code, identity.Description, DateTimeOffset.UtcNow, request.Note, cacheObject.InstanceState);
                TrackChanges(cacheObject, identity.Code);
                if (cacheObject.InstanceState == InstanceState.Progress)
                    await UpdateDataBaseAsync(cacheObject, identity, false, false, true, cacheObject.ModifiedBy, cacheObject.ModifiedOn);

                await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
                {
                    Company = identity.CompanyId,
                    User = identity.Code,
                    Action = TrailAction.InstanceSignatureCanceled,
                    Date = DateTimeOffset.UtcNow,
                    CheckListCode = cacheObject.InstanceCode,
                    CheckListID = cacheObject.ID,
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
                    CheckListCode = cacheObject.InstanceCode,
                    CheckListID = cacheObject.ID,
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
            var checkListURL = Instance.GetUrl(_configurationService, cacheObject.Code);
            foreach (var Email in Emails)
                await _queueService.SendObjectAsync(QueueName.Email,
                    new EmailMessage(identity.CompanyId, "Signature notification", message,
                        Email.Address, new List<Tuple<string, string>> {
                            new Tuple<string, string> ("User", Email.Description),
                            new Tuple<string, string> ("CheckListCode", $"<a href='{checkListURL}' target='_blank'>{cacheObject.InstanceCode}</a>")
                    })
                    {
                        TrailMessage = new EmailTrailMessage(identity.Code, cacheObject.InstanceCode, cacheObject.ID, new EmailTrailMessage.EmailTrailInformation
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

            if (cacheObject.Instance.ReadOnly)
                return;

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
                    cacheObject.NeedToCache = cacheObject.NeedToSave = true;
                    await UpdateDataBaseAsync(cacheObject, identity, false, false, true, cacheObject.ModifiedBy, cacheObject.ModifiedOn);
                }

                await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
                {
                    Company = identity.CompanyId,
                    User = identity.Code,
                    Action = TrailAction.InstanceQuitted,
                    Date = DateTimeOffset.UtcNow,
                    CheckListCode = cacheObject.InstanceCode,
                    CheckListID = cacheObject.ID
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
                    Page.Lock.LastUpdate = DateTimeOffset.UtcNow;

            cacheObject.NeedToSave = true;
            cacheObject.NeedToCache = true;
            cacheObject.ModifiedBy = userCode;
            cacheObject.ModifiedOn = DateTimeOffset.UtcNow;
        }

        private async Task<List<Item>> HandleExternalSourceRowChange(InstanceCacheObject cacheObject, IDefaultIdentity identity, int? itemId, Dictionary<string, string> row)
        {
            if (!itemId.HasValue)
                throw new InstanceException("Item id not sent");

            var field = cacheObject.Instance.Items.ContainsKey(itemId.Value) ? cacheObject.Instance.Items[itemId.Value] as Field : throw new InstanceException($"Field {itemId} not found");

            if (field.ReadOnly || !cacheObject.Instance.ExternalSources.TryGetValue(field.ExternalSourceName, out var ExternalSource))
                return null;

            ExternalSource.Values = row;
            List<Item> AffectedElements;

            if (field.IterationItem)
            {
                AffectedElements = null;
                var Roots = new HashSet<Tuple<DependencyNode, DependencyRelation.Properties>>();
                foreach (var ExternalSourceDependencyRelation in ExternalSource.DependentRelations)
                {
                    var IteratorModelItem = (Item)ExternalSourceDependencyRelation.GetNode(cacheObject.Instance);
                    Item IterationItem = IteratorModelItem.FindIterationItems(field.Iteration).Single();
                    IteratorModelItem.MarkNodeAsChanged(cacheObject.Instance, Roots, DependencyRelation.Properties.Value);

                    //the fields bound to the external source of the iteration that contains field are update directly
                    IterationItem.Update(DependencyRelation.Properties.Value);

                    //the iterator model item are marked as changed by the iteration child involved and then skipped to avoid all the iterations of the iterator are updated with last value
                    IteratorModelItem.MarkNodeAsUnchanged(DependencyRelation.Properties.Value);


                    /*
                    var IteratorModelItem = (Item)ExternalSourceDependencyRelation.GetNode(cacheObject.Instance);
                    Item IterationItem = IteratorModelItem.FindIterationItems(field.Iteration).Single();

                    //the siblings are update directly
                    IterationItem.Update(DependencyRelation.Properties.Value);

                    //the iterator model item are marked as changed and then skipped to avoid all the iterations of the iterator are updated
                    IteratorModelItem = IterationItem.FindIteratorModelItem();
                    var Roots = new HashSet<Tuple<DependencyNode, DependencyRelation.Properties>>();
                    IteratorModelItem.MarkNodeAsChanged(cacheObject.Instance, Roots, DependencyRelation.Properties.Value);
                    IteratorModelItem.MarkNodeAsUnchanged(DependencyRelation.Properties.Value);
                    DependencyNode.UpdateNodes(cacheObject.Instance, Roots);
                    */
                }

                DependencyNode.UpdateNodes(cacheObject.Instance, Roots);
            }
            else
            {
                AffectedElements = new List<Item>();
                foreach (var ExternalSourceDependencyRelation in ExternalSource.DependentRelations)
                {
                    var AffectedField = (Field)ExternalSourceDependencyRelation.GetNode(cacheObject.Instance);
                    if (!AffectedField.IteratorModelItem) //other scope
                        AffectedElements.Add(AffectedField); 
                }
            }

            await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
            {
                Company = cacheObject.CompanyId,
                User = identity.Code,
                Action = TrailAction.InstanceExternalSourceRowChanged,
                Date = DateTimeOffset.UtcNow,
                CheckListCode = cacheObject.InstanceCode,
                CheckListID = cacheObject.ID,
                Information = JsonConvert.SerializeObject(new
                {
                    ID = itemId,
                    Values = row
                })
            });

            TrackChanges(cacheObject, identity.Code);
            return AffectedElements;
        }

        private async Task HandleImport(InstanceCacheObject cacheObject, IDefaultIdentity identity, int? itemId, string externalSourceName, List<string> columns, List<List<string>> rows)
        {
            if (!itemId.HasValue)
                throw new InstanceException("Item id not sent");

            var group = cacheObject.Instance.Items.ContainsKey(itemId.Value) ? cacheObject.Instance.Items[itemId.Value] as Group : null;
            if (group == null)
                throw new InstanceException($"Group {itemId} not found");

            if (group.ReadOnly || !cacheObject.Instance.ExternalSources.TryGetValue(externalSourceName, out var ExternalSource))
                return;

            var rowDictionary = columns.ToDictionary(k => k, v => default(string));
            var iterationsId = new List<int>(rows?.Count ?? 0);
            var Roots = new HashSet<Tuple<DependencyNode, DependencyRelation.Properties>>();
            group.ModifiedBy = identity.Code;
            group.ModifiedOn = DateTimeOffset.UtcNow;
            group.MarkNodeAsChanged(cacheObject.Instance, Roots, DependencyRelation.Properties.Iterations);
            var iterationIndex = group.Iterations.Count(i => !i.Removed);
            if (rows != null)
                foreach (var row in rows)
                {
                    var iteration = group.AddIteration();
                    group.SetupIterationIntexes(iteration, ref iterationIndex);
                    iterationsId.Add(iteration.Id);
                    for (var index = 0; index < columns.Count; ++index)
                        rowDictionary[columns[index]] = row[index];
                    ExternalSource.Values = rowDictionary;

                    var IterationChildren = new List<DependencyNode>();
                    iteration.GetAllChildren(IterationChildren);
                    IterationChildren = IterationChildren.Where(x => x != null).ToList();

                    foreach (var Child in IterationChildren.Cast<Element>())
                    {
                        //the children of the added iterations are all marked as changed
                        Child.MarkAllAsChanged(Roots);

                        //the fields bound to the external source are update directly
                        if (Child is Field Field && Field.ExternalSourceName == ExternalSource.Code)
                        {
                            Field.Update(DependencyRelation.Properties.Value);

                            //the iterator model item are marked as changed by the iteration child involved and then skipped to avoid all the iterations of the iterator are updated with last value
                            var IteratorModelItem = ((Item)Child).FindIteratorModelItem();
                            IteratorModelItem.MarkNodeAsUnchanged(DependencyRelation.Properties.Value);
                        }
                    }
                }
            group.SetupIterationsIndexes();

            DependencyNode.UpdateNodes(cacheObject.Instance, Roots);

            //call audit trail only after the rest of the process has worked
            for (var rowIndex = 0; rows != null && rowIndex < rows.Count; ++rowIndex)
            {
                for (var columnIndex = 0; columnIndex < columns.Count; ++columnIndex)
                    rowDictionary[columns[columnIndex]] = rows[rowIndex][columnIndex];

                await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
                {
                    Company = cacheObject.CompanyId,
                    User = identity.Code,
                    Action = TrailAction.InstanceExternalSourceRowImported,
                    Date = DateTimeOffset.UtcNow,
                    CheckListCode = cacheObject.InstanceCode,
                    CheckListID = cacheObject.ID,
                    Information = JsonConvert.SerializeObject(new
                    {
                        Iteration = iterationsId[rowIndex],
                        Source = externalSourceName,
                        Values = rowDictionary
                    })
                });
            }

            TrackChanges(cacheObject, identity.Code);
        }

        private async Task<Element> HandleUploadAttachment(
            InstanceCacheObject cacheObject,
            IDefaultIdentity identity,
            int? itemId,
            HttpFile file
        )
        {
            if (!itemId.HasValue)
                throw new InstanceException("Item id not sent");

            if (!cacheObject.Instance.Items.TryGetValue(itemId.Value, out var item) || !(item is Attachment attachment))
                throw new InstanceException($"Attachment {itemId} not found");

            if (item.ReadOnly)
                return null;

            if (file == null)
                throw new InstanceException("No attachment has been uploaded");

            attachment = (Attachment)attachment.GetTarget();

            var extension = Path.GetExtension(file.FileName);
            if (attachment.FileType == Attachment.FileTypes.Images)
            {
                if (!file.MediaType.StartsWith("image/"))
                    throw new InstanceException("The attachment has to be an image");

                file.Buffer = ImageAdapter.RotateAndCompressImage(file.Buffer);
            }
            else if (attachment.FileType == Attachment.FileTypes.Pdf)
            {
                if (extension?.ToLower() != ".pdf")
                    throw new InstanceException("The attachment has to be a pdf document");
            }
            else
                throw new InstanceException("File type not supported");

            var maxSize = await _settingService.InstancesAttachmentMaxSizeBytesAsync(identity.CompanyId);
            if (file.Buffer.Length > maxSize)
                throw new InstanceException($"The file exceed the file size limit of {maxSize.BytesCountToString()}");

            var key = $"{GetAttachmentKeyPrefix(identity, cacheObject.Code)}{attachment.Id}{extension}";
            using (var memoryStream = new MemoryStream(file.Buffer))
            {
                var success = await _storageService.PutAsync(_configurationService.InstancesStorageBucket, key, memoryStream);
                if (!success)
                    throw new InstanceException("Impossible to upload the attachment to the storage");
            }

            attachment.Key = key;
            attachment.ModifiedBy = identity.Code;
            attachment.ModifiedOn = DateTimeOffset.UtcNow;

            await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
            {
                Company = cacheObject.CompanyId,
                User = identity.Code,
                Action = TrailAction.InstanceValueChanged,
                Date = DateTimeOffset.UtcNow,
                CheckListCode = cacheObject.InstanceCode,
                CheckListID = cacheObject.ID,
                Information = JsonConvert.SerializeObject(new
                {
                    ID = itemId,
                    file.FileName
                })
            });

            TrackChanges(cacheObject, identity.Code);

            return attachment;
        }

        private async Task<Element> HandleAttachInstance(
            InstanceCacheObject cacheObject,
            IDefaultIdentity identity,
            int? itemId,
            long? instance
        )
        {
            if (!itemId.HasValue)
                throw new InstanceException("Item id not sent");

            if (!cacheObject.Instance.Items.TryGetValue(itemId.Value, out var item) || !(item is InstanceLink instanceLink))
                throw new InstanceException($"Instance link {itemId} not found");

            if (item.ReadOnly)
                return !instance.HasValue ? throw new InstanceException($"The Instance Link {itemId} is not editable and cannot be linked to the instance {instance.Value}") : default(Element);

            var instanceRecord = instance.HasValue ? await _instancesRepository.GetAsync(identity.CompanyId, instance.Value) : null;

            instanceLink.Attach(instance, instanceRecord?.Code,
                instance.HasValue ? new InstanceLinkRecord
                {
                    ID = instanceRecord.ID,
                    Code = Instance.GetCode(instanceRecord.PrototypeName, instanceRecord.Version, instanceRecord.PrototypeVersions.VersionState, instanceRecord.Year, instanceRecord.Number),
                    Description = instanceRecord.Description,
                    State = instanceRecord.InstanceState,
                    DateTime = DateTimeOffset.UtcNow,
                    User = identity.Code
                } : new InstanceLinkRecord
                {
                    DateTime = DateTimeOffset.UtcNow,
                    User = identity.Code
                }
            );

            if (instance.HasValue)
            {
                if (cacheObject.ID.HasValue)
                {
                    _instancesRepository.AddRelation(cacheObject.ID.Value, itemId.Value, instance.Value);
                    await _instancesRepository.SaveChangesAsync();
                }
                cacheObject.Instance.ChildInstances[itemId.Value] = instance.Value;
            }
            else
            {
                if (cacheObject.ID.HasValue)
                {
                    await _instancesRepository.DeleteRelation(cacheObject.ID.Value, itemId.Value);
                    await _instancesRepository.SaveChangesAsync();
                }
                cacheObject.Instance.ChildInstances.Remove(itemId.Value);
            }

            await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
            {
                Company = cacheObject.CompanyId,
                User = identity.Code,
                Action = TrailAction.InstanceValueChanged,
                Date = DateTimeOffset.UtcNow,
                CheckListCode = cacheObject.InstanceCode,
                CheckListID = cacheObject.ID,
                Information = JsonConvert.SerializeObject(new
                {
                    ID = itemId,
                    Instance = instanceRecord?.ID,
                    Code = instanceRecord == null ? null : Instance.GetCode(instanceRecord.PrototypeName, instanceRecord.Version, instanceRecord.PrototypeVersions.VersionState, instanceRecord.Year, instanceRecord.Number),
                    instanceRecord?.Description,
                    State = instanceRecord?.InstanceState
                })
            });

            TrackChanges(cacheObject, identity.Code);

            return instanceLink;
        }

        private async Task<Element> HandleRefreshAttachedInstance(
            InstanceCacheObject cacheObject,
            IDefaultIdentity identity,
            int? itemId,
            long? instance
        )
        {
            if (!itemId.HasValue)
                throw new InstanceException("Item id not sent");

            if (!cacheObject.Instance.Items.TryGetValue(itemId.Value, out var item) || !(item is InstanceLink instanceLink))
                throw new InstanceException($"Instance link {itemId} not found");

            if (item.ReadOnly)
                return null;

            if (!instanceLink.AttachedInstanceID.HasValue)
                throw new InstanceException("No instance attached");

            if (instanceLink.AttachedInstanceID.Value != instance.Value)
                return null; //the parent instance is not attached to the child {instance} anymore

            var instanceRecord = await _instancesRepository.GetAsync(identity.CompanyId, instanceLink.AttachedInstanceID.Value);

            var newInstanceLinkRecord = new InstanceLinkRecord
            {
                ID = instanceRecord.ID,
                Code = Instance.GetCode(instanceRecord.PrototypeName, instanceRecord.Version, instanceRecord.PrototypeVersions.VersionState, instanceRecord.Year, instanceRecord.Number),
                Description = instanceRecord.Description,
                State = instanceRecord.InstanceState,
                DateTime = DateTimeOffset.UtcNow,
                User = identity.Code
            };

            if (
                instanceLink.Record.Description == newInstanceLinkRecord.Description
                && instanceLink.Record.State == newInstanceLinkRecord.State
            )
                return null; //no changes found

            instanceLink.Attach(instanceLink.AttachedInstanceID.Value, instanceRecord.Code, newInstanceLinkRecord);

            await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
            {
                Company = cacheObject.CompanyId,
                User = identity.Code,
                Action = TrailAction.InstanceValueChanged,
                Date = DateTimeOffset.UtcNow,
                CheckListCode = cacheObject.InstanceCode,
                CheckListID = cacheObject.ID,
                Information = JsonConvert.SerializeObject(new
                {
                    ID = itemId,
                    Instance = instanceRecord.ID,
                    Code = Instance.GetCode(instanceRecord.PrototypeName, instanceRecord.Version, instanceRecord.PrototypeVersions.VersionState, instanceRecord.Year, instanceRecord.Number),
                    instanceRecord.Description,
                    State = instanceRecord.InstanceState
                })
            });

            TrackChanges(cacheObject, identity.Code);

            return instanceLink;
        }

        #endregion
    }
}