using System;
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

		public async Task<InstanceCacheObject> CreateAsync(long companyId, string userCode, int prototype, int version)
		{
			try
			{
				var instance = await CreateInstanceFromPrototypeAsync(prototype, version);
				var cacheObject = new InstanceCacheObject
				{
					CompanyId = companyId,
					Instance = instance,
					Code = _hashService.HashStringToString($"{prototype}-{version}-{Guid.NewGuid()}"),
					Prototype = prototype,
					Version = version,
					InstanceState = InstanceState.Draft,
					CreatedBy = userCode,
					CreatedOn = DateTimeOffset.UtcNow
				};

				await _instanceCache.SetAsync(cacheObject.Code, cacheObject);

				instance.Setup();

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

		public async Task ConfirmAsync(string userCode, string code)
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

		public Instance GetCheckList(Instance instance)
		{
			//prepare model for client if any change is needed
			return instance;
		}

		private async Task<Instance> CreateInstanceFromPrototypeAsync(int prototype, int version)
		{
			try
			{
				//TODO: add companyId (modify prototype endpoint)
				var httpResponse = await _client.GetAsync($"Prototypes/Prototype/GetAsync/{prototype}/{version}");
				var responseData = await httpResponse.Content.ReadAsStringAsync();

				var response = JsonConvert.DeserializeObject<ResponseMessage<GetPrototypeResponse>>(responseData, _converter);

				return response.Data.CheckList;
			}
			catch (Exception e)
			{
				Logger.Error(e, "Unable to retrieve the prototype");
				throw new InstanceException("Unable to retrieve the prototype", e);
			}
		}

		private async Task ValidateExpressionsAsync(InstanceCacheObject cacheObject)
		{
			//TODO complete implementation
			var instances = await _instancesRepository.GetInstancesAsync<Common.Data.Instances, int>(cacheObject.CompanyId,
				i => i,
				i =>
					i.Prototype == cacheObject.Prototype
					&& i.InstanceState == InstanceState.Progress
			);

			foreach (var instance in instances.Data)
			{
				
			}
		}
	}
}