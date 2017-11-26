using System;
using System.Threading.Tasks;
using CheckListManager.Common.Logic.Cache;
using CheckListManager.Common.Logic.Json;
using CheckListManager.Instances.Logic.Models;
using CheckListManager.Instances.Logic.Models.CheckLists;
using NLog;

namespace CheckListManager.Instances.Logic.Services
{
	public class InstanceCacheService : IInstanceCacheService
	{
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

		private const string RegionName = "Instance";

		private readonly IRedisCache _redisCache;
		private readonly CheckListJsonConverter _converter;

		public InstanceCacheService(IRedisCache redisCache, CheckListJsonConverter converter)
		{
			_redisCache = redisCache;
			_converter = converter;
		}

		public async Task<InstanceCacheObject> GetAsync(string code, bool includeData = true)
		{
			var instance = await _redisCache.GetAsync<InstanceCacheObject>($"Instance:{code}:Metadata", RegionName);
			if (instance != null && includeData)
			{
				instance.Instance = await _redisCache.GetAsync<Instance>($"Instance:{code}:Data", RegionName, _converter);
			}

			return instance;
		}

		public async Task SetAsync(string code, InstanceCacheObject instance)
		{
			try
			{
				_redisCache.BeginTransaction();

				await _redisCache.SetAsync($"Instance:{code}:Metadata", instance, RegionName);
				if (instance.Instance != null)
				{
					await _redisCache.SetAsync($"Instance:{code}:Data", instance.Instance, RegionName);
				}

				var score = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

				await _redisCache.SortedSetElementInsertAsync("Instances", instance.Code, score, RegionName);

				await _redisCache.CommitTransactionAsync();
			}
			catch (Exception exception)
			{
				_redisCache.RollbackTransaction();
				Logger.Error(exception);
				throw;
			}
		}

		public async Task RemoveAsync(string code)
		{
			try
			{
				_redisCache.BeginTransaction();

				await _redisCache.RemoveAsync($"Instance:{code}:Metadata", RegionName);
				await _redisCache.RemoveAsync($"Instance:{code}:Data", RegionName);
				await _redisCache.SortedSetElementRemoveAsync("Instances", code, RegionName);

				await _redisCache.CommitTransactionAsync();
			}
			catch (Exception exception)
			{
				_redisCache.RollbackTransaction();
				Logger.Error(exception);
				throw;
			}
		}

		public Task TouchInstanceAsync(string code, double? score = null)
		{
			score = score ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds();
			return _redisCache.SortedSetElementInsertAsync("Instances", code, score.Value, RegionName);
		}
	}
}