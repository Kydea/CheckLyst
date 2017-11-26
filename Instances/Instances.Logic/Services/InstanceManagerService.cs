using System;
using System.Linq;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Network;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.Json;
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
		private readonly IHashService _hashService;
		private readonly IInstancesRepository _instancesRepository;
		private readonly CheckListJsonConverter _converter;
		private readonly IInstanceCacheService _instanceCache;

		public InstanceManagerService(IServiceClient client, IHashService hashService, IInstancesRepository instancesRepository, CheckListJsonConverter converter, IInstanceCacheService instanceCache)
		{
			_client = client;
			_hashService = hashService;
			_instancesRepository = instancesRepository;
			_converter = converter;
			_instanceCache = instanceCache;
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
			{
				throw new InstanceException("Instance not found");
			}

			cacheObject = CreateInstanceFromDatabase(companyId, instance);
			if (cacheObject.InstanceState == InstanceState.Progress)
			{
				await _instanceCache.SetAsync(cacheObject.Code, cacheObject);
			}

			return cacheObject;
		}

		public async Task<InstanceCacheObject> CreateAsync(long companyId, string userCode, int prototype, int version)
		{
			try
			{
				var source = await CreateInstanceFromPrototypeAsync(prototype, version);
				var cacheObject = new InstanceCacheObject
				{
					CompanyId = companyId,
					Instance = source.CheckList,
					Code = _hashService.HashStringToString($"{prototype}-{version}-{Guid.NewGuid()}"),
					PrototypeType = source.PrototypeType,
					Prototype = prototype,
					Version = version,
					InstanceState = InstanceState.Draft,
					CreatedBy = userCode,
					CreatedOn = DateTimeOffset.UtcNow
				};

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

		public async Task OpenAsync(string userCode, string code)
		{
			try
			{
				var cacheObject = await _instanceCache.GetAsync(code);
				if (cacheObject == null)
				{
					throw new InstanceException("Instance not found");
				}
				if (cacheObject.Id.HasValue)
				{
					throw new InstanceException("Instance already confirmed");
				}
				if (cacheObject.InstanceState != InstanceState.Draft)
				{
					throw new InstanceException("Instance state not valid");
				}

				cacheObject.InstanceState = InstanceState.Progress;
				cacheObject.Instance.Update();

				if (!cacheObject.Instance.Header.Valid)
				{
					throw new InstanceException("Header not valid");
				}
				await ValidateExpressionsAsync(cacheObject);

				var row = new Common.Data.Instances
				{
					Data = JsonConvert.SerializeObject(cacheObject.Instance, _converter),
					Code = code,
					InstanceState = cacheObject.InstanceState,
					Prototype = cacheObject.Prototype,
					Version = cacheObject.Version,
					Year = DateTimeOffset.UtcNow.Year,
					CreatedBy = cacheObject.CreatedBy,
					CreatedOn = cacheObject.CreatedOn,
					ModifiedBy = userCode,
					ModifiedOn = DateTimeOffset.UtcNow
				};

				row = await _instancesRepository.CreateInstanceAsync(row);

				cacheObject.Id = row.ID;
				cacheObject.Number = row.Number;
				cacheObject.Description = row.Description;
				cacheObject.PrototypeCode = row.PrototypeCode;
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
				Logger.Error(e, "Unable to create the instance");
				throw new InstanceException("Unable to create the instance", e);
			}
		}

		public async Task CloseAsync(string code)
		{
			try
			{
				var cacheObject = await _instanceCache.GetAsync(code);
				if (cacheObject == null)
				{
					return;
				}
				if (cacheObject.InstanceState == InstanceState.Draft)
				{
					await _instanceCache.RemoveAsync(code);
					return;
				}
				if (cacheObject.InstanceState == InstanceState.Progress)
				{
					//Data = JsonConvert.SerializeObject(cacheObject.Instance, _converter)
					//					ModifiedBy = userCode,
					//ModifiedOn = DateTimeOffset.UtcNow

					//save;
					//				row = await _instancesRepository.CreateInstanceAsync(row);
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

		public Instance GetCheckList(string userCode, bool canEdit, InstanceCacheObject instance)
		{
			canEdit = canEdit && instance.InstanceState == InstanceState.Progress;

			instance.Instance.Update();
			instance.Instance.UpdateReadOnly(userCode, canEdit);

			return instance.Instance;
		}

		private async Task<GetPrototypeResponse> CreateInstanceFromPrototypeAsync(int prototype, int version)
		{
			try
			{
				var httpResponse = await _client.GetAsync($"Prototypes/Prototype/Get/{prototype}/{version}");
				var responseData = await httpResponse.Content.ReadAsStringAsync();

				var response = JsonConvert.DeserializeObject<ResponseMessage<GetPrototypeResponse>>(responseData, _converter);

				return response.Data;
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

		private InstanceCacheObject CreateInstanceFromDatabase(long companyId, Common.Data.Instances instance)
		{
			return new InstanceCacheObject
			{
				CompanyId = companyId,
				Instance = JsonConvert.DeserializeObject<Instance>(instance.Data, _converter),
				Code = instance.Code,
				Prototype = instance.Prototype,
				Version = instance.Version,
				InstanceState = instance.InstanceState,
				CreatedBy = instance.CreatedBy,
				CreatedOn = instance.CreatedOn,
				Id = instance.ID,
				Number = instance.Number,
				Description = instance.Description,
				PrototypeCode = instance.PrototypeCode,
				ModifiedOn = instance.ModifiedOn,
				ModifiedBy = instance.ModifiedBy,
			};
		}
	}
}