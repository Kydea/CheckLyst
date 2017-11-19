using System;
using System.Collections.Generic;
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
			var instance = await _redisCache.GetAsync<InstanceCacheObject>($"Instance:Metadata:{code}", RegionName);
			if (includeData)
			{
				instance.Instance = await _redisCache.GetAsync<Instance>($"Instance:Data:{code}", RegionName, _converter);
			}

			return instance;
		}

		public Task<IEnumerable<InstanceCacheObject>> GetByUserAsync(string userCode)
		{
			throw new NotImplementedException();
			//return _redisCache.GetAsync<IEnumerable<InstanceCacheObject>>($"InstancesByUser:{userCode}", RegionName, _converter);
		}

		public async Task SetAsync(string code, string userCode, InstanceCacheObject instance)
		{
			try
			{
				_redisCache.BeginTransaction();

				await _redisCache.SetAsync($"Instance:Metadata:{code}", instance, RegionName);
				if (instance.Instance != null)
				{
					await _redisCache.SetAsync($"Instance:Data:{code}", instance.Instance, RegionName);
				}

				var score = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

				await _redisCache.SortedSetElementInsertAsync($"Instance:Users:{code}", userCode, score, RegionName);
				await _redisCache.SortedSetElementInsertAsync("Instances", instance.Code, score, RegionName);
				await _redisCache.SortedSetElementInsertAsync($"InstancesByUser:{userCode}", instance.Code, score, RegionName);

				await _redisCache.CommitTransactionAsync();
			}
			catch (Exception exception)
			{
				_redisCache.RollbackTransaction();
				Logger.Error(exception);
				throw;
			}
		}

		public async Task RemoveAsync(string code, string userCode)
		{
			try
			{
				_redisCache.BeginTransaction();

				await _redisCache.SortedSetElementRemoveAsync($"Instance:Users:{code}", userCode, RegionName);
				await _redisCache.SortedSetElementRemoveAsync($"InstancesByUser:{userCode}", code, RegionName);

				await _redisCache.CommitTransactionAsync();

				var count = await _redisCache.SortedSetCountAsync($"Instance:Users:{code}", RegionName);
				if (count == 0)
				{
					_redisCache.BeginTransaction();

					await _redisCache.RemoveAsync($"Instance:Metadata:{code}", RegionName);
					await _redisCache.RemoveAsync($"Instance:Data:{code}", RegionName);
					await _redisCache.RemoveAsync($"Instance:Users:{code}", RegionName);
					await _redisCache.SortedSetElementRemoveAsync("Instances", code, RegionName);

					await _redisCache.CommitTransactionAsync();
				}
			}
			catch (Exception exception)
			{
				_redisCache.RollbackTransaction();
				Logger.Error(exception);
				throw;
			}
		}
	}
}