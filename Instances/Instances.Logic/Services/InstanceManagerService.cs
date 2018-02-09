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
using CheckListManager.Common.Logic.Queue;
using CheckListManager.Common.Logic.Trail;
using CheckListManager.Common.Logic.FTP;

namespace CheckListManager.Instances.Logic.Services
{
    public class InstanceManagerService : IInstanceManagerService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IServiceClient _client;
        private readonly IInstancesRepository _instancesRepository;
        private readonly CheckListJsonConverter _converter;
        private readonly IInstanceCacheService _instanceCache;
        private readonly IConfigurationService _configurationService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IWorkFlowsService _workFlowService;
        private readonly IQueueService _queueService;
        private readonly ICommonRepository _commonRepository;

        public InstanceManagerService(IServiceClient client,
            IInstancesRepository instancesRepository, CheckListJsonConverter converter, IInstanceCacheService instanceCache,
            IConfigurationService configurationService, IAuthenticationService authenticationService, IWorkFlowsService workFlowService,
            IQueueService queueService, ICommonRepository commonRepository)
        {
            _client = client;
            _instancesRepository = instancesRepository;
            _converter = converter;
            _instanceCache = instanceCache;
            _configurationService = configurationService;
            _authenticationService = authenticationService;
            _workFlowService = workFlowService;
            _queueService = queueService;
            _commonRepository = commonRepository;
        }

        public async Task<InstanceCacheObject> GetAsync(IDefaultIdentity identity, string code)
        {
            var cacheObject = await GetAsync(identity.CompanyId, identity.Code, code);

            if (!HasPermission(identity, "View", cacheObject.VersionState, cacheObject.Department, cacheObject.PrototypeType))
                throw new InstanceException("Access denied");

            await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
            {
                Company = identity.CompanyId,
                User = identity.Code,
                Action = TrailAction.InstanceOpened,
                Date = DateTimeOffset.UtcNow,
                CheckList = cacheObject.InstanceCode
            });

            return cacheObject;
        }

        public async Task<InstanceCacheObject> GetAsync(long company, string userCode, string code)
        {
            var cacheObject = await _instanceCache.GetAsync(code);
            if (cacheObject == null)
            {
                var instance = await _instancesRepository.GetByCodeAsync(company, code);
                if (instance == null)
                    throw new InstanceException("Instance not found");

                cacheObject = CreateInstanceFromDatabase(company, instance);
                if (cacheObject.InstanceState == InstanceState.Progress)
                {
                    if (cacheObject.Instance.Pages.Count() > 0)
                        ((Page)cacheObject.Instance.Pages[0]).Lock = new Lock(userCode);
                    await _instanceCache.SetAsync(cacheObject.Code, cacheObject);
                }
            }
            else if (cacheObject.Instance.Pages.Any() && ((Page)cacheObject.Instance.Pages.First()).Lock == null)
            {
                UnlockPages(cacheObject.Instance, userCode);
                ((Page)cacheObject.Instance.Pages.First()).Lock = new Lock(userCode);
                await _instanceCache.SetAsync(cacheObject.Code, cacheObject);
            }
            else
                await _instanceCache.TouchInstanceAsync(code);

            return cacheObject;
        }

        public async Task<InstanceCacheObject> GetAsync(long company, long ID)
        {
            var instance = await _instancesRepository.GetByIDAsync(company, ID);
            if (instance == null)
                throw new InstanceException("Instance not found");
            var cacheObject = CreateInstanceFromDatabase(company, instance);
            return cacheObject;
        }

        public async Task<InstanceCacheObject> CreateAsync(IDefaultIdentity identity, string userCode, long prototype, int version)
        {
            try
            {
                var source = await GetPrototype(prototype, version);
                if (!source.State.In(VersionState.Draft, VersionState.InSimulation, VersionState.Active))
                    throw new InstanceException($"Invalid state of the prototype version {prototype}.{version}: {source.State}");

                if (
                    !HasPermission(identity, "Create", source.State, source.Department, source.PrototypeType)
                    && !HasPermission(identity, "Edit", source.State, source.Department, source.PrototypeType)
                )
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
                    Version = version,
                    VersionState = source.State,
                    InstanceState = InstanceState.Draft,
                    CreatedBy = userCode,
                    CreatedOn = DateTimeOffset.UtcNow
                };
                cacheObject.Instance.Setup(true);

                await _instanceCache.SetAsync(cacheObject.Code, cacheObject);

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

        public async Task<InstanceCacheObject> OpenAsync(string code, IDefaultIdentity identity)
        {
            try
            {
                var cacheObject = await _instanceCache.GetAsync(code);
                if (cacheObject == null)
                    throw new InstanceException("Instance not found");
                if (cacheObject.Id.HasValue)
                    throw new InstanceException("Instance already saved");
                if (cacheObject.InstanceState != InstanceState.Draft)
                    throw new InstanceException("Instance state not valid");
                if (
                    !HasPermission(identity, "Create", cacheObject.VersionState, cacheObject.Department, cacheObject.PrototypeType)
                    && !HasPermission(identity, "Edit", cacheObject.VersionState, cacheObject.Department, cacheObject.PrototypeType)
                )
                    throw new InstanceException("Access denied");

                cacheObject.InstanceState = InstanceState.Progress;
                cacheObject.Instance.Update(cacheObject);

                if (!cacheObject.Instance.Header.Valid)
                    throw new InstanceException("Header not valid");
                await ValidateUniqueExpressionsAsync(cacheObject);

                var row = new Common.Data.Instances
                {
                    Company = identity.CompanyId,
                    Data = JsonConvert.SerializeObject(cacheObject.Instance, _converter),
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

                row = await _instancesRepository.CreateInstanceAsync(row);

                cacheObject.Id = row.ID;
                cacheObject.Year = row.Year;
                cacheObject.Number = row.Number;
                cacheObject.ModifiedOn = row.ModifiedOn;
                cacheObject.ModifiedBy = row.ModifiedBy;
                if (cacheObject.Instance.Pages.Count() > 0)
                    ((Page)cacheObject.Instance.Pages[0]).Lock = new Lock(identity.Code);
                cacheObject.IsDirty = false;

                _instancesRepository.SaveChildren(row, CreateTags(cacheObject), CreateExpressionValues(cacheObject));
                await _instancesRepository.SaveChangesAsync();

                await _instanceCache.SetAsync(cacheObject.Code, cacheObject);

                if (!cacheObject.VersionState.In(VersionState.Draft, VersionState.InSimulation))
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

        public async Task SaveAsync(string code, IDefaultIdentity identity)
        {
            try
            {
                var cacheObject = await _instanceCache.GetAsync(code, false);
                if (cacheObject == null)
                    throw new InstanceException("Instance not found");
                if (cacheObject.InstanceState != InstanceState.Progress)
                    throw new InstanceException("Instance state not valid");
                if (!HasPermission(identity, "Edit", cacheObject.VersionState, cacheObject.Department, cacheObject.PrototypeType))
                    throw new InstanceException("Access denied");

                await SaveAsync(cacheObject, identity, false, false, false);
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

        public async Task<InstanceCacheObject> CompleteAsync(string code, IDefaultIdentity identity)
        {
            try
            {
                var cacheObject = await _instanceCache.GetAsync(code);
                if (cacheObject == null)
                    throw new InstanceException("Instance not found");
                if (cacheObject.InstanceState != InstanceState.Progress)
                    throw new InstanceException("Invalid instance state");
                if (!HasPermission(identity, "Edit", cacheObject.VersionState, cacheObject.Department, cacheObject.PrototypeType))
                    throw new InstanceException("Access denied");

                cacheObject.Instance.Update(cacheObject);
                var UnvalidItems = cacheObject.Instance.Items.Values.Where(i => !i.Valid && !i.IteratorModelItem);
                if (UnvalidItems.Any())
                    throw new InstanceException($"The following items are not valid: {string.Join(", ", UnvalidItems.Select(i => i.Id.ToString()))}");

                cacheObject = await SaveAsync(cacheObject, identity, true, true, false);
                cacheObject.Instance.UpdateReadOnly(identity.Code, InstanceState.Completed);

                if (!cacheObject.VersionState.In(VersionState.Draft, VersionState.InSimulation))
                    await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
                    {
                        Company = identity.CompanyId,
                        User = identity.Code,
                        Action = TrailAction.InstanceStateChanged,
                        Date = DateTimeOffset.UtcNow,
                        CheckList = cacheObject.InstanceCode,
                        Information = cacheObject.InstanceState
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

        public async Task DeleteAsync(string code, IDefaultIdentity identity, string note)
        {
            try
            {
                var cacheObject = await _instanceCache.GetAsync(code, false);
                if (cacheObject == null)
                    throw new InstanceException("Instance not found");
                if (!cacheObject.InstanceState.In(InstanceState.Draft, InstanceState.Progress))
                    throw new InstanceException("Instance state not valid");
                if (cacheObject.VersionState == VersionState.Active && cacheObject.InstanceState == InstanceState.Progress && string.IsNullOrWhiteSpace(note))
                    throw new InstanceException("Provide a note for the deletion");
                if (
                    !(cacheObject.InstanceState == InstanceState.Draft && HasPermission(identity, "Edit", cacheObject.VersionState, cacheObject.Department, cacheObject.PrototypeType))
                    && !HasPermission(identity, "Delete", cacheObject.VersionState, cacheObject.Department, cacheObject.PrototypeType)
                )
                    throw new InstanceException("Access denied");

                await DeleteAsync(cacheObject, identity.Code, note);

                if (!cacheObject.VersionState.In(VersionState.Draft, VersionState.InSimulation))
                    await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
                    {
                        Company = identity.CompanyId,
                        User = identity.Code,
                        Action = TrailAction.InstanceStateChanged,
                        Date = DateTimeOffset.UtcNow,
                        CheckList = cacheObject.InstanceCode,
                        Information = note
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

        private async Task SuspendAsync(InstanceCacheObject cacheObject, IDefaultIdentity identity)
        {
            try
            {
                switch (cacheObject.InstanceState)
                {
                    case InstanceState.Draft:
                        await _instanceCache.RemoveAsync(cacheObject.Code);
                        break;
                    case InstanceState.Progress:
                        await SaveAsync(cacheObject, identity, true, false, false);
                        break;
                    default:
                        throw new InstanceException("Invalid state: " + cacheObject.InstanceState);
                }
            }
            catch (InstanceException)
            {
                throw;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Unable to close the instance");
                throw new InstanceException("Unable to close the instance", e);
            }
        }

        private void AddTags(Dictionary<string, InstanceTags> Tags, long InstanceID, ICheckListGroup Group)
        {
            if (Group != null)
                foreach (var Item in Group.Items)
                    if (Item is ICheckListGroup)
                        AddTags(Tags, InstanceID, (ICheckListGroup)Item);
                    else if (!string.IsNullOrWhiteSpace(Item.Description) && (Item.TypeName == "Field" || Item.TypeName == "Signature") && !Tags.ContainsKey(Item.Description))
                        Tags[Item.Description] = (new InstanceTags { Instance = InstanceID, Tag = Item.Description, Value = (Item.TypeName == "Field" ? ((Field)Item).RawValue : ((ICheckListSignature)Item).SignedBy) ?? String.Empty });
        }

        private IEnumerable<InstanceTags> CreateTags(InstanceCacheObject cacheObject)
        {
            var Tags = new Dictionary<string, InstanceTags>();
            AddTags(Tags, cacheObject.Id.Value, cacheObject.Instance?.Header);
            return Tags.Values;
        }

        private IEnumerable<InstanceExpressionValues> CreateExpressionValues(InstanceCacheObject cacheObject)
        {
            return cacheObject.Instance.Expressions.Where(Expression => !Expression.Value.Value.IsNull).Select(e => new InstanceExpressionValues
            {
                Instance = cacheObject.Id.Value,
                Code = e.Key,
                Value = e.Value.Value.SerializedValue
            });
        }

        private async Task<InstanceCacheObject> SaveAsync(InstanceCacheObject cacheObject, IDefaultIdentity identity, bool close, bool complete, bool alreadyLocked)
        {
            LockObject lockObject = null;

            try
            {
                if (!alreadyLocked)
                {
                    lockObject = await _instanceCache.LockInstanceAsync(cacheObject.Code);
                    if (!lockObject.IsAcquired)
                        throw new InstanceException("Instance is currently being updated by another user");
                    cacheObject = await _instanceCache.GetAsync(cacheObject.Code);
                }

                var row = await _instancesRepository.GetByCodeAsync(cacheObject.CompanyId, cacheObject.Code);
                if (row == null)
                {
                    await _instanceCache.RemoveAsync(cacheObject.Code);
                    throw new InstanceException("Instance not found");
                }

                cacheObject.Instance.Update(cacheObject);
                row.Data = JsonConvert.SerializeObject(cacheObject.Instance, _converter);
                row.Description = cacheObject.Instance.Description;
                row.FileName = 
                    !cacheObject.Instance.FTPExport ? null :
                    !string.IsNullOrWhiteSpace(cacheObject.Instance.FileName) ? cacheObject.Instance.FileName :
                    $"Instance{cacheObject.InstanceCode}";
                row.ModifiedOn = cacheObject.ModifiedOn;
                row.ModifiedBy = cacheObject.ModifiedBy;

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
                        Date = row.ModifiedOn.Value,
                        User = row.ModifiedBy,
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
                        _instanceCache.TouchInstanceAsync(cacheObject.Code)
                    );
                return cacheObject;
            }
            finally
            {
                lockObject?.Dispose();
            }
        }

        private async Task DeleteAsync(InstanceCacheObject cacheObject, string userCode, string Note)
        {
            if (cacheObject.InstanceState == InstanceState.Progress)
            {
                var row = await _instancesRepository.GetByCodeAsync(cacheObject.CompanyId, cacheObject.Code);
                if (row == null)
                    throw new InstanceException("Instance not found");

                if (cacheObject.VersionState == VersionState.Active)
                {
                    row.ModifiedOn = DateTimeOffset.UtcNow;
                    row.ModifiedBy = userCode;
                    row.InstanceState = InstanceState.Deleted;

                    row.InstancesHistory.Add(new InstancesHistory
                    {
                        Date = row.ModifiedOn.Value,
                        User = row.ModifiedBy,
                        State = row.InstanceState,
                        Note = Note
                    });
                }
                else
                    _instancesRepository.DeleteInstance(row);

                await _instancesRepository.SaveChangesAsync();
            }
            await _instanceCache.RemoveAsync(cacheObject.Code);
        }

        #region Events

        public async Task<InstanceCacheObject> HandleEventAsync(IDefaultIdentity identity, InstanceEventRequest request)
        {
            LockObject lockObject = null;

            try
            {
                var cacheObject = await _instanceCache.GetAsync(request.Code, false);
                if (cacheObject == null)
                    throw new InstanceException("Instance not found");

                if (cacheObject.InstanceState == InstanceState.Progress)
                {
                    //Lock and reload the data to be sure it's the most recent one
                    lockObject = await _instanceCache.LockInstanceAsync(request.Code);
                    if (!lockObject.IsAcquired)
                        throw new InstanceException("Instance is currently being updated by another user");
                }

                cacheObject = await _instanceCache.GetAsync(request.Code);

                var LockUser = request.Type == "Signature" ? request.Username : null; //A signer may sign also if the page is locked by the main user (the lock is for concurrency purpose, not for security)
                GetCheckList(identity, "Edit", cacheObject, LockUser);

                if (cacheObject.Instance.ReadOnly)
                    throw new InstanceException("This instance cannot be modified");

                switch (request.Type)
                {
                    case "PageChange":
                        await HandlePageChangeAsync(cacheObject, identity.Code, request.ItemId);
                        break;
                    case "ValueChange":
                        await HandleValueChangeAsync(cacheObject, identity.Code, request.ItemId, request.Value);
                        break;
                    case "ValueCopy":
                        await HandleValueCopyAsync(cacheObject, identity.Code, request.ItemId);
                        break;
                    case "Add":
                        await HandleAddAsync(cacheObject, identity.Code, request.ItemId);
                        break;
                    case "Remove":
                        await HandleRemoveAsync(cacheObject, identity.Code, request.ItemId);
                        break;
                    case "Up":
                        await HandleMoveAsync(cacheObject, identity.Code, request.ItemId, -1);
                        break;
                    case "Down":
                        await HandleMoveAsync(cacheObject, identity.Code, request.ItemId, 1);
                        break;
                    case "Signature":
                        await HandleSignatureAsync(cacheObject, identity, request);
                        break;
                    case "SignatureCheck":
                        HandleSignatureCheck(cacheObject, request.Username, request.ItemId);
                        break;
                    case "SignatureCancelation":
                        await HandleSignatureCancelationAsync(cacheObject, identity, request);
                        break;
                    case "UnlockPage":
                        await HandleUnlockPageAsync(cacheObject, identity.Code, request.ItemId);
                        break;
                    case "CountDown":
                        break;
                    case "SkipCountDown":
                        await HandleSkipCountDown(cacheObject, request.Username, request.ItemId);
                        break;
                    case "Refresh":
                        break;
                    case "Quit":
                        await HandleQuitAsync(cacheObject, identity);
                        break;
                    //case "ExternalSourceRowChange":
                    default:
                        throw new InstanceException($"Event type {request.Type} not handled.");
                }

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
                lockObject?.Dispose();
            }
        }

        private async Task HandlePageChangeAsync(InstanceCacheObject cacheObject, string userCode, int? itemId)
        {
            if (!itemId.HasValue)
                throw new InstanceException("Item id not sent");

            var page = cacheObject.Instance.Pages.Count > itemId.Value ? cacheObject.Instance.Pages[itemId.Value] as Page : null;
            if (page == null)
                throw new InstanceException("Page not found");

            UnlockPages(cacheObject.Instance, userCode);
            if (page.Lock == null)
                page.Lock = new Lock(userCode);
            await Update(cacheObject, userCode);
        }

        private async Task HandleValueChangeAsync(InstanceCacheObject cacheObject, string userCode, int? itemId, string value)
        {
            if (!itemId.HasValue)
                throw new InstanceException("Item id not sent");

            var field = cacheObject.Instance.Items.ContainsKey(itemId.Value) ? cacheObject.Instance.Items[itemId.Value] as Field : null;
            if (field == null)
                throw new InstanceException($"Field {itemId} not found");

            if (field.ReadOnly)
                return;

            if ((value ?? string.Empty) != (field.Value ?? string.Empty) && field.Validate(value))
            {
                field.History.Add(new FieldRecord
                {
                    DateTime = DateTimeOffset.UtcNow,
                    User = userCode,
                    Value = value
                });
                field.Value = value;

                await Update(cacheObject, userCode);
            }
        }

        private async Task HandleValueCopyAsync(InstanceCacheObject cacheObject, string userCode, int? itemId)
        {
            if (!itemId.HasValue)
                throw new InstanceException("Item id not sent");

            var field = cacheObject.Instance.Items.ContainsKey(itemId.Value) ? cacheObject.Instance.Items[itemId.Value] as Field : null;
            if (field == null)
                throw new InstanceException($"Field {itemId} not found");

            if (field.ReadOnly)
                return;

            string value = null;

            if (field.IterationItem && field.Iteration.Index > 1)
            {
                var PreviuosIteration = field.Iteration.Group.Iterations[field.Iteration.Index - 2];
                var ItemIndex = field.Iteration.Items.Contains(field) ? field.Iteration.Items.IndexOf(field) : -1;
                var PreviuosField = ItemIndex < 0 ? null : (Field)PreviuosIteration.Items[field.Iteration.Items.IndexOf(field)];
                value = PreviuosField?.Value ?? String.Empty;
            }

            if ((value ?? string.Empty) != (field.Value ?? string.Empty) && field.Validate(value))
            {
                field.History.Add(new FieldRecord
                {
                    DateTime = DateTimeOffset.UtcNow,
                    User = userCode,
                    Value = value
                });
                field.Value = value;

                await Update(cacheObject, userCode);
            }
        }

        private async Task HandleAddAsync(InstanceCacheObject cacheObject, string userCode, int? itemId)
        {
            if (!itemId.HasValue)
                throw new InstanceException("Item id not sent");

            var group = cacheObject.Instance.Items.ContainsKey(itemId.Value) ? cacheObject.Instance.Items[itemId.Value] as Group : null;
            if (group == null)
                throw new InstanceException("Group not found");

            if (!group.Iterator || group.DisableAdd)
                throw new InstanceException("Invalid operation");

            if (!group.ReadOnly)
            {
                group.AddIterations(1);
                await Update(cacheObject, userCode);
            }
        }

        private async Task HandleRemoveAsync(InstanceCacheObject cacheObject, string userCode, int? itemId)
        {
            if (!itemId.HasValue)
                throw new InstanceException("Item id not sent");

            var iteration = cacheObject.Instance.Items.ContainsKey(itemId.Value) ? cacheObject.Instance.Items[itemId.Value] as Iteration : null;
            if (iteration == null)
                throw new InstanceException("Group not found");

            if (iteration.Group.DisableRemove)
                throw new InstanceException("Invalid operation");

            if (!iteration.ReadOnly)
            {
                ((Group)iteration.Group).Remove(iteration);
                await Update(cacheObject, userCode);
            }
        }

        private async Task HandleMoveAsync(InstanceCacheObject cacheObject, string userCode, int? itemId, int direction)
        {
            if (!itemId.HasValue)
                throw new InstanceException("Item id not sent");

            var iteration = cacheObject.Instance.Items.ContainsKey(itemId.Value) ? cacheObject.Instance.Items[itemId.Value] as Iteration : null;
            if (iteration == null)
                throw new InstanceException("Group not found");

            if (iteration.Group.DisableMove)
                throw new InstanceException("Invalid operation");

            if (!iteration.ReadOnly)
            {
                ((Group)iteration.Group).Move(iteration, direction);
                await Update(cacheObject, userCode);
            }
        }

        private void HandleSignatureCheck(InstanceCacheObject cacheObject, string userCode, int? itemId)
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

        private async Task HandleSignatureAsync(InstanceCacheObject cacheObject, IDefaultIdentity identity, InstanceEventRequest request)
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

            if (request.Simple.Value && string.IsNullOrEmpty(request.Password))
                throw new InstanceException("Provide your password");
            if (!request.Simple.Value && string.IsNullOrEmpty(request.Pin))
                throw new InstanceException("Provide your PIN");

            signature.TrySign(request.Username);

            try
            {
                if (request.Simple.Value)
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

            signature.Sign(identity.Code, identity.Description, DateTimeOffset.UtcNow);
            if (cacheObject.InstanceState == InstanceState.Progress)
                await SaveAsync(cacheObject, identity, false, false, true);
            await Update(cacheObject, request.Username);

            if (!cacheObject.VersionState.In(VersionState.Draft, VersionState.InSimulation))
                await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
                {
                    Company = identity.CompanyId,
                    User = identity.Code,
                    Action = TrailAction.InstanceSignatureIssued,
                    Date = DateTimeOffset.UtcNow,
                    CheckList = cacheObject.InstanceCode,
                    Information = request.ItemId.ToString()
                });
        }


        private async Task HandleSignatureCancelationAsync(InstanceCacheObject cacheObject, IDefaultIdentity identity, InstanceEventRequest request)
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
                throw new InstanceException("Provide a cancelation note");

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
            if (cacheObject.InstanceState == InstanceState.Progress)
                await SaveAsync(cacheObject, identity, false, false, true);
            await Update(cacheObject, request.Username);

            if (!cacheObject.VersionState.In(VersionState.Draft, VersionState.InSimulation))
                await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
                {
                    Company = identity.CompanyId,
                    User = identity.Code,
                    Action = TrailAction.InstanceSignatureCanceled,
                    Date = DateTimeOffset.UtcNow,
                    CheckList = cacheObject.InstanceCode,
                    Information = JsonConvert.SerializeObject(new
                    {
                        Id = request.ItemId,
                        Note = request.Note
                    })
                });
        }

        private async Task HandleUnlockPageAsync(InstanceCacheObject cacheObject, string userCode, int? itemId)
        {
            if (!itemId.HasValue)
                throw new InstanceException("Item id not sent");

            var page = cacheObject.Instance.Pages.Count > itemId.Value ? cacheObject.Instance.Pages[itemId.Value] as Page : null;
            if (page == null)
                throw new InstanceException("Page not found");

            page.Lock = new Lock(userCode);

            await Update(cacheObject, userCode);
        }

        private async Task HandleSkipCountDown(InstanceCacheObject cacheObject, string userCode, int? itemId)
        {
            if (!itemId.HasValue)
                throw new InstanceException("Item id not sent");

            var item = cacheObject.Instance.Items.ContainsKey(itemId.Value) ? cacheObject.Instance.Items[itemId.Value] : null;
            if (item == null)
                throw new InstanceException("Signature not found");

            if (!cacheObject.VersionState.In(VersionState.Draft, VersionState.InSimulation))
                throw new InvalidOperationException("A count down can be skipped only for preview and simulation instances");

            item.CountDownToCode = null;
            await _instanceCache.SetAsync(cacheObject.Code, cacheObject);
        }

        private async Task HandleQuitAsync(InstanceCacheObject cacheObject, IDefaultIdentity identity)
        {
            try
            {
                if (!HasPermission(identity, "View", cacheObject.VersionState, cacheObject.Department, cacheObject.PrototypeType))
                    throw new InstanceException("Access denied");

                UnlockPages(cacheObject.Instance, identity.Code);
                await Update(cacheObject, identity.Code);

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

        private async Task Update(InstanceCacheObject cacheObject, string userCode)
        {
            cacheObject.TrackChanges(userCode);
            await _instanceCache.SetAsync(cacheObject.Code, cacheObject);
        }

        #endregion

        public bool HasPermission(IDefaultIdentity identity, string function, string PrototypeVersionState, long Department, long PrototypeType, string Tag = null)
        {
            return
                PrototypeVersionState.In(VersionState.Draft, VersionState.InSimulation)
                    && (
                        identity.HasPermission("Prototypes", "Edit", Department)
                        || identity.HasPermission("Prototypes", "Edit", Department, PrototypeType)
                    )
                || !PrototypeVersionState.In(VersionState.Draft, VersionState.InSimulation)
                && (
                    identity.HasPermission("Instances", function, Department, tag: Tag)
                    || identity.HasPermission("Instances", function, Department, PrototypeType, Tag)
                );
        }

        public Instance GetCheckList(IDefaultIdentity identity, string functionGroup, InstanceCacheObject cacheObject, string LockUser = null)
        {
            cacheObject.Instance.ReadOnly =
                !cacheObject.InstanceState.In(InstanceState.Draft, InstanceState.Progress)
                || !HasPermission(identity, functionGroup, cacheObject.VersionState, cacheObject.Department, cacheObject.PrototypeType);

            cacheObject.Instance.Update(cacheObject);
            cacheObject.Instance.UpdateReadOnly(LockUser ?? identity.Code, cacheObject.InstanceState);

            return cacheObject.Instance;
        }

        public async Task ClearPendingInstances(CancellationToken cancellationToken = new CancellationToken())
        {
            var score = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - _configurationService.InstanceCloseTimeout * 60;
            var codes = await _instanceCache.GetInstancesWithRangeAsync(stop: score);

            foreach (var code in codes)
            {
                try
                {
                    var instance = await _instanceCache.GetAsync(code, false);
                    if (instance == null)
                        continue;

                    if (instance.IsDirty)
                        await SuspendAsync(instance, null);
                    else
                        await _instanceCache.RemoveAsync(code);
                }
                catch (Exception exception)
                {
                    Logger.Error(exception);
                }
            }
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
                    var messase = JsonConvert.DeserializeObject<ResponseMessage>(responseData);
                    throw new InstanceException(messase.Messages.FirstOrDefault() ?? "Unable to retrieve the prototype");
                }
                var response = JsonConvert.DeserializeObject<GetPrototypeResponse>(responseData, _converter);
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

        private async Task ValidateUniqueExpressionsAsync(InstanceCacheObject cacheObject)
        {
            var instances = await _instancesRepository.GetInstancesWithExpressions(cacheObject.Prototype, cacheObject.Version);

            foreach (var instance in instances)
            {
                var list = cacheObject.Instance.Expressions.Where(e =>
                    e.Value.Unique
                    && !e.Value.Value.IsNull
                    && instance.InstanceExpressionValues.Any(
                        e2 =>
                            e2.Code == e.Key
                            && e2.Value == e.Value.Value?.SerializedValue
                    )
                ).ToList();

                if (list.Count > 0)
                {
                    var expression = list.First();
                    throw new InstanceException(
                        $"The value {expression.Value.Value.SerializedValue} for the expression {expression.Key} has been found in the in progress instance {(instance.Year > 0 ? instance.Year.ToString() : instance.PrototypeVersions.VersionState == VersionState.Draft ? "TEST" : "SE")} - {instance.Number}");
                }
            }
        }

        private static void UnlockPages(ICheckList instance, string userCode)
        {
            foreach (var Page in instance.Pages.Cast<Page>())
                if (Page.Lock?.User == userCode)
                    Page.Lock = null;
        }

        private InstanceCacheObject CreateInstanceFromDatabase(long companyId, Common.Data.Instances instance)
        {
            var cacheObject = new InstanceCacheObject
            {
                CompanyId = companyId,
                Instance = JsonConvert.DeserializeObject<Instance>(instance.Data, _converter),
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
            cacheObject.Instance.Setup(false);
            return cacheObject;
        }

        public async Task<List<HistoryRow>> History(DefaultIdentity identity, long id)
        {
            var Instance = await _instancesRepository.GetByIDAsync(identity.CompanyId, id);
            if (Instance == null)
                throw new InstanceException("Instance not found");
            if (!HasPermission(identity, "View", Instance.PrototypeVersions.VersionState, Instance.Prototypes.Department, Instance.Prototypes.PrototypeType))
                throw new UnauthorizedAccessException();

            return await Task.FromResult(Instance.InstancesHistory.OrderBy(h => h.ID).Select(h => new HistoryRow
            {
                ID = h.ID,
                Date = h.Date,
                User = $"{h.User} - {_commonRepository.GetUser(identity.CompanyId, h.User)?.Description ?? "(unknown user)"}",
                State = h.WorkFlowApplications == null ? h.State : null,
                ProposedState = h.WorkFlowApplications == null || h.WorkFlowApplications.Approved.HasValue ? null : InstanceState.Confirmed,
                Approved = h.WorkFlowApplications?.Approved,
                Note = h.Note,
                Steps = h.WorkFlowApplications?.WorkFlowApplicationSteps.OrderBy(s => s.Step).ThenBy(s => s.ID).Select(s => new HistoryStep
                {
                    ID = s.ID,
                    Step = s.Step + 1,
                    Date = s.Date,
                    User = s.User,
                    Level = $"({string.Join(", ", s.WorkFlowApplicationStepTags.Select(t => t.Tag))})",
                    Reason = s.Reason,
                    Comment = s.Comment,
                    Approved = s.Approved,
                    AppointedUser = s.User == null ? string.Join(", ", _commonRepository.GetAppointedUsers(identity.CompanyId, "Instances", "Confirm", Instance.Prototypes.Department, Instance.Prototypes.PrototypeType, s.WorkFlowApplicationStepTags.Select(t => t.Tag)).Select(u => u.Code)) : null
                }).ToList()
            }).ToList());
        }
    }
}