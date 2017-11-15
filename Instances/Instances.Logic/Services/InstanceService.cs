using System;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Network;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.Cache;
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
	public class InstanceService : IInstanceService
	{
		private const string RegionName = "Instance";

		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
		private static readonly TimeSpan DefaultInstanceExpiry = TimeSpan.FromMinutes(30); //TODO: add as configuration

		private readonly IServiceClient _client;
		private readonly IRedisCache _redisCache;
		private readonly IHashService _hashService;
		private readonly IInstancesRepository _instancesRepository;
		private readonly CheckListJsonConverter _converter;

		public InstanceService(IServiceClient client, IRedisCache redisCache, IHashService hashService, IInstancesRepository instancesRepository, CheckListJsonConverter converter)
		{
			_client = client;
			_redisCache = redisCache;
			_hashService = hashService;
			_instancesRepository = instancesRepository;
			_converter = converter;
		}

		public async Task<CreateInstanceResponse> Create(int prototype, int version)
		{
			try
			{
				var instance = await CreateInstanceFromPrototypeAsync(prototype, version);

				var cacheObject = new CacheObject
				{
					Instance = instance,
					Code = _hashService.HashStringToString($"{prototype}-{version}-{Guid.NewGuid()}"),
					Prototype = prototype,
					Version = version
				};

				await _redisCache.SetAsync($"Instance:{cacheObject.Code}", cacheObject, RegionName, DefaultInstanceExpiry);

				var response = new CreateInstanceResponse
				{
					Code = cacheObject.Code,
					CheckList = cacheObject.Instance
				};

				return response;
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

		public async Task<SaveInstanceResponse> SaveAsync(long companyId, string userCode, string code, Instance instance)
		{
			try
			{
				var cacheObject = await _redisCache.GetAsync<CacheObject>($"Instance:{code}", RegionName);
				Common.Data.Instances row;

				if (cacheObject == null)
				{
					row = await _instancesRepository.GetByCodeAsync(companyId, code);

					throw new InstanceException("Instance not found");
				}

				if (!cacheObject.Id.HasValue)
				{
					row = new Common.Data.Instances
					{
						Data = JsonConvert.SerializeObject(instance),
						Code = code,
					};

					await _instancesRepository.CreateInstanceAsync(row);
				}
				else
				{
					//row.Data = JsonConvert.SerializeObject(instance);
					//await _instancesRepository.UpdateInstanceAsync(userCode, row);
				}

				return new SaveInstanceResponse
				{
					Code = code,
					CheckList = instance
				};
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

		private async Task<Instance> CreateInstanceFromPrototypeAsync(int prototype, int version)
		{
			try
			{
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
	}
}