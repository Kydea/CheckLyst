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

		public InstanceManagerService(IServiceClient client,
			IInstancesRepository instancesRepository, CheckListJsonConverter converter, IInstanceCacheService instanceCache,
			IConfigurationService configurationService)
		{
			_client = client;
			_instancesRepository = instancesRepository;
			_converter = converter;
			_instanceCache = instanceCache;
			_configurationService = configurationService;
		}

		public async Task<InstanceCacheObject> GetAsync(long companyId, string code)
		{
			var cacheObject = await _instanceCache.GetAsync(code);
			if (cacheObject != null)
			{
				await _instanceCache.TouchInstanceAsync(code);
				return cacheObject;
			}

			var instance = await _instancesRepository.GetByCodeAsync(companyId, code);
			if (instance == null)
				throw new InstanceException("Instance not found");

			cacheObject = CreateInstanceFromDatabase(companyId, instance);
			if (cacheObject.InstanceState == InstanceState.Progress)
				await _instanceCache.SetAsync(cacheObject.Code, cacheObject);

			return cacheObject;
		}

		public async Task<InstanceCacheObject> CreateAsync(long companyId, string userCode, long prototype, int version)
		{
			try
			{
				var source = await CreateInstanceFromPrototypeAsync(prototype, version);
				var cacheObject = new InstanceCacheObject
				{
					CompanyId = companyId,
					Instance = source.CheckList,
					Code = $"{prototype:X}-{version:X}-{Guid.NewGuid()}",
					Department = source.Department,
					DepartmentName = source.DepartmentName,
					PrototypeType = source.PrototypeType,
					PrototypeTypeName = source.PrototypeTypeName,
					Prototype = prototype,
					PrototypeName = source.Name,
					Version = version,
					VersionState = source.State,
					InstanceState = InstanceState.Draft,
					CreatedBy = userCode,
					CreatedOn = DateTimeOffset.UtcNow
				};
				cacheObject.Instance.Setup();

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

		public async Task OpenAsync(string code, IDefaultIdentity identity)
		{
			try
			{
				var cacheObject = await _instanceCache.GetAsync(code);
				if (cacheObject == null)
					throw new InstanceException("Instance not found");
				if (cacheObject.Id.HasValue)
					throw new InstanceException("Instance already confirmed");
				if (cacheObject.InstanceState != InstanceState.Draft)
					throw new InstanceException("Instance state not valid");
				if (!identity.HasPermission("Instance", "Edit", cacheObject.Department, cacheObject.PrototypeType))
					throw new InstanceException("Access denied");

				cacheObject.InstanceState = InstanceState.Progress;
				cacheObject.Instance.Update();

				if (!cacheObject.Instance.Header.Valid)
					throw new InstanceException("Header not valid");
				await ValidateExpressionsAsync(cacheObject);

				var row = new Common.Data.Instances
				{
					Data = JsonConvert.SerializeObject(cacheObject.Instance, _converter),
					Code = code,
					InstanceState = cacheObject.InstanceState,
					Prototype = cacheObject.Prototype,
					PrototypeName = cacheObject.PrototypeName,
					Version = cacheObject.Version,
					Year = DateTimeOffset.UtcNow.Year,
					CreatedBy = cacheObject.CreatedBy,
					CreatedOn = cacheObject.CreatedOn,
					ModifiedBy = identity.Code,
					ModifiedOn = DateTimeOffset.UtcNow
				};

				row = await _instancesRepository.CreateInstanceAsync(row);

				cacheObject.Id = row.ID;
				cacheObject.Number = row.Number;
				cacheObject.Description = row.Description;
				cacheObject.PrototypeName = row.PrototypeName;
				cacheObject.InstanceState = row.InstanceState;
				cacheObject.ModifiedOn = row.ModifiedOn;
				cacheObject.ModifiedBy = row.ModifiedBy;
				cacheObject.IsDirty = false;

				await _instanceCache.SetAsync(cacheObject.Code, cacheObject);
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
				if (!cacheObject.Id.HasValue)
					throw new InstanceException("Instance already confirmed");
				if (cacheObject.InstanceState != InstanceState.Progress)
					throw new InstanceException("Instance state not valid");
				if (!identity.HasPermission("Instance", "Edit", cacheObject.Department, cacheObject.PrototypeType))
					throw new InstanceException("Access denied");

				await SaveAsync(cacheObject, identity.Code, false, true);
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

		public async Task CloseAsync(string code, IDefaultIdentity identity)
		{
			try
			{
				var cacheObject = await _instanceCache.GetAsync(code, false);
				if (cacheObject == null)
					return;

				if (identity.HasPermission("Instance", "Edit", cacheObject.Department, cacheObject.PrototypeType))
					await CloseAsync(cacheObject, identity.Code, false);
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

		private async Task CloseAsync(InstanceCacheObject cacheObject, string userCode, bool force)
		{
			try
			{
				// ReSharper disable once ConvertIfStatementToSwitchStatement
				if (cacheObject.InstanceState == InstanceState.Draft)
				{
					await _instanceCache.RemoveAsync(cacheObject.Code);
					return;
				}
				if (cacheObject.InstanceState == InstanceState.Progress)
				{
					await SaveAsync(cacheObject, userCode, force, false);
				}
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

		private async Task SaveAsync(InstanceCacheObject cacheObject, string userCode, bool close, bool save)
		{
			LockObject lockObject = null;

			try
			{
				lockObject = await _instanceCache.LockInstanceAsync(cacheObject.Code);
				if (!lockObject.IsAcquired)
					throw new InstanceException("Impossible to obtain the lock");

				cacheObject = await _instanceCache.GetAsync(cacheObject.Code);

				var row = await _instancesRepository.GetByCodeAsync(cacheObject.CompanyId, cacheObject.Code);
				if (row == null)
					throw new InstanceException("Instance not found");

				UnlockPages(cacheObject.Instance, userCode);
				row.Data = JsonConvert.SerializeObject(cacheObject.Instance, _converter);
				row.ModifiedOn = cacheObject.ModifiedOn;
				row.ModifiedBy = cacheObject.ModifiedBy;

				if (save)
					await Task.WhenAll(
						_instancesRepository.UpdateInstanceAsync(userCode, row),
						_instanceCache.TouchInstanceAsync(cacheObject.Code)
					);
				else if (close)
					await Task.WhenAll(
						_instancesRepository.UpdateInstanceAsync(userCode, row),
						_instanceCache.RemoveAsync(cacheObject.Code)
					);
				else
					await Task.WhenAll(
						_instancesRepository.UpdateInstanceAsync(userCode, row),
						_instanceCache.TouchInstanceAsync(cacheObject.Code, 1)
					);
			}
			finally
			{
				lockObject?.Dispose();
			}
		}

		#region Events

		public async Task<InstanceCacheObject> HandleEventAsync(DefaultIdentity identity, InstanceEventRequest request)
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
						throw new InstanceException("Instance is being updated by another user");
				}

				cacheObject = await _instanceCache.GetAsync(request.Code);

				var canEdit =
					identity.HasPermission("Instances", "Edit", cacheObject.Department, cacheObject.PrototypeType) &&
					cacheObject.InstanceState.In(InstanceState.Draft, InstanceState.Progress);
				var instance = GetCheckList(identity.Code, canEdit, cacheObject);

				switch (request.Type)
				{
					case "PageChange":
						await HandlePageChangeAsync(cacheObject, instance, identity.Code, canEdit, request.Item);
						break;
					case "ValueChange":
						await HandleValueChangeAsync(cacheObject, instance, identity.Code, canEdit, request.Item, request.Data);
						break;
					case "Add":
						await HandleAddAsync(cacheObject, instance, identity.Code, canEdit, request.Item);
						break;
					case "Remove":
						await HandleRemoveAsync(cacheObject, instance, identity.Code, canEdit, request.Item);
						break;
					case "Up":
						await HandleMoveAsync(cacheObject, instance, identity.Code, canEdit, request.Item, -1);
						break;
					case "Down":
						await HandleMoveAsync(cacheObject, instance, identity.Code, canEdit, request.Item, 1);
						break;
					//case "UnlockPage":
					//case "Refresh":
					//case "CountDown":
					//case "Signature":
					//case "SignatureCheck":
					//case "SignatureCancelation":
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

		private async Task HandlePageChangeAsync(InstanceCacheObject cacheObject, Instance instance, string userCode, bool canEdit, int? itemId)
		{
			if (!canEdit)
				return;

			if (!itemId.HasValue)
				throw new InstanceException("Item id not sent");

			var page = instance.Pages.Count > itemId.Value ? instance.Pages[itemId.Value] as Page : null;
			if (page == null)
				throw new InstanceException("Page not found");

			UnlockPages(instance, userCode);
			if (!page.IsLocked)
			{
				page.Lock = new Lock(userCode);
				await Update(cacheObject, instance, userCode);
			}
		}

		private async Task HandleValueChangeAsync(InstanceCacheObject cacheObject, Instance instance, string userCode, bool canEdit, int? itemId, string value)
		{
			CheckPermission(canEdit);
			if (!itemId.HasValue)
				throw new InstanceException("Item id not sent");

			var field = instance.Items.ContainsKey(itemId.Value) ? instance.Items[itemId.Value] as Field : null;
			if (field == null)
				throw new InstanceException("Field not found");

			if (!field.IsReadOnly)
			{
				if ((value ?? string.Empty) != (field.Value?.Value ?? string.Empty) && field.Validate(value))
				{
					//TODO: add audit
					field.History.Add(new FieldRecord
					{
						DateTime = DateTimeOffset.UtcNow,
						User = userCode,
						Value = value
					});
					field.Value = new Common.Logic.Models.RawValue(value);

					await Update(cacheObject, instance, userCode);
				}
			}
		}

		private async Task HandleAddAsync(InstanceCacheObject cacheObject, Instance instance, string userCode, bool canEdit, int? itemId)
		{
			CheckPermission(canEdit);
			if (!itemId.HasValue)
				throw new InstanceException("Item id not sent");

			var group = instance.Items.ContainsKey(itemId.Value) ? instance.Items[itemId.Value] as Group : null;
			if (group == null)
				throw new InstanceException("Group not found");

			if (!group.IsReadOnly)
			{
				group.AddIteration();
				await Update(cacheObject, instance, userCode);
			}
		}

		private async Task HandleRemoveAsync(InstanceCacheObject cacheObject, Instance instance, string userCode, bool canEdit, int? itemId)
		{
			CheckPermission(canEdit);
			if (!itemId.HasValue)
			{
				throw new InstanceException("Item id not sent");
			}

			var iteration = instance.Items.ContainsKey(itemId.Value) ? instance.Items[itemId.Value] as Iteration : null;
			if (iteration == null)
				throw new InstanceException("Group not found");

			if (!iteration.IsReadOnly)
			{
				((Group)iteration.Group).Remove(iteration);
				await Update(cacheObject, instance, userCode);
			}
		}

		private async Task HandleMoveAsync(InstanceCacheObject cacheObject, Instance instance, string userCode, bool canEdit, int? itemId, int direction)
		{
			CheckPermission(canEdit);
			if (!itemId.HasValue)
				throw new InstanceException("Item id not sent");

			var iteration = instance.Items.ContainsKey(itemId.Value) ? instance.Items[itemId.Value] as Iteration : null;
			if (iteration == null)
				throw new InstanceException("Group not found");

			if (!iteration.IsReadOnly)
			{
				((Group)iteration.Group).Move(iteration, direction);
				await Update(cacheObject, instance, userCode);
			}
		}

		// ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
		// ReSharper disable once UnusedParameter.Local
		private static void CheckPermission(bool canEdit)
		{
			if (!canEdit)
				throw new InstanceException("This instance cannot be modified");
		}

		private async Task Update(InstanceCacheObject cacheObject, Instance instance, string userCode)
		{
			cacheObject.Instance = instance;
			cacheObject.AsChanged(userCode);
			await _instanceCache.SetAsync(cacheObject.Code, cacheObject);
		}

		#endregion

		public Instance GetCheckList(string userCode, bool canEdit, InstanceCacheObject instance)
		{
			canEdit = canEdit && instance.InstanceState.In(InstanceState.Draft, InstanceState.Progress);

			instance.Instance.Update();
			instance.Instance.UpdateReadOnly(userCode, canEdit, instance.InstanceState);

			return instance.Instance;
		}

		public async Task ClearPendingInstances(CancellationToken cancellationToken = new CancellationToken())
		{
			var score = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - _configurationService.InstanceCloseTimeout * 60;
			var codes = await _instanceCache.GetInstancesWithRangeAsync(stop: score);

			foreach (var code in codes)
			{
				var instance = await _instanceCache.GetAsync(code, false);
				if (instance == null)
					continue;

				if (instance.IsDirty)
					await CloseAsync(instance, null, true);
				else
					await _instanceCache.RemoveAsync(code);
			}
		}

		private async Task<GetPrototypeResponse> CreateInstanceFromPrototypeAsync(long prototype, int version)
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

		private async Task ValidateExpressionsAsync(InstanceCacheObject cacheObject)
		{
			var instances = await _instancesRepository.GetInstancesWithExpressions(InstanceState.Progress);

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
					throw new Exception(
						$"The value {expression.Value.Value.SerializedValue} for the expression {expression.Key} has been found in the in progress instance {instance.Year} - {instance.Number}");
				}
			}
		}

		private static void UnlockPages(ICheckList instance, string userCode)
		{
			var pages = instance.Pages.Cast<Page>().Where(p => userCode == null && p.Lock != null || p.Lock != null && p.Lock.User == userCode);
			foreach (var p in pages)
				p.Lock = null;
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
				PrototypeType = instance.Prototypes.Department,
				PrototypeTypeName = instance.Prototypes.PrototypeTypes.Name,
				Prototype = instance.Prototype,
				PrototypeName = instance.PrototypeName,
				Version = instance.Version,
				VersionState = instance.PrototypeVersions.VersionState,
				InstanceState = instance.InstanceState,
				CreatedBy = instance.CreatedBy,
				CreatedOn = instance.CreatedOn,
				Id = instance.ID,
				Year = instance.Year,
				Number = instance.Number,
				Description = instance.Description,
				ModifiedOn = instance.ModifiedOn,
				ModifiedBy = instance.ModifiedBy
			};
			cacheObject.Instance.Setup();
			return cacheObject;
		}
	}
}