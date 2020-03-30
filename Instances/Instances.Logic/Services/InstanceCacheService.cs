using System;
using System.Threading.Tasks;
using CheckListManager.Common.Logic.Cache;
using CheckListManager.Common.Logic.Json;
using CheckListManager.Instances.Logic.Exceptions;
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

		public async Task<InstanceCacheObject> GetAsync(string code, bool includeData)
		{
			var cacheObject = await _redisCache.GetAsync<InstanceCacheObject>($"Instance:{code}:Metadata", RegionName);
			if (cacheObject != null && includeData)
			{
				cacheObject.Instance = await _redisCache.GetAsync<Instance>($"Instance:{code}:Data", RegionName, _converter);
				if (cacheObject.Instance == null) throw new InstanceException("Instance data missing");
				cacheObject.Instance.Setup(false);
			}

			return cacheObject;
		}

		public async Task SetAsync(InstanceCacheObject cacheObject)
		{
			try
			{
				_redisCache.BeginTransaction();

				await _redisCache.SetAsync($"Instance:{cacheObject.Code}:Metadata", cacheObject, RegionName);
				if (cacheObject.Instance != null)
					await _redisCache.SetAsync($"Instance:{cacheObject.Code}:Data", cacheObject.Instance, RegionName);

				var score = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

				await _redisCache.SortedSetElementInsertAsync("Instances", cacheObject.Code, score, RegionName);

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

		public Task TouchAsync(string code)
			=>_redisCache.SortedSetElementInsertAsync("Instances", code, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), RegionName);

		public Task<LockObject> LockAsync(string code)
			=> _redisCache.LockAsync($"Instance:{code}:Lock", RegionName);

		public Task<string[]> GetInstancesInRangeAsync(double start = double.NegativeInfinity, double stop = double.PositiveInfinity)
			=> _redisCache.SortedSetGetAsync("Instances", start, stop, RegionName);
    }
}