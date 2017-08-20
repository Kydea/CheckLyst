using System;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Helpers;
using StackExchange.Redis;

namespace CheckListManager.Common.Logic.Cache
{
	public class RedisCache : IRedisCache
	{
		protected IDatabase Database { get; }

		public RedisCache(IRedisSetup redisSetup)
		{
			Database = redisSetup.Connection.GetDatabase();
		}

		public object Get(string key, string regionName = null)
			=> Database.StringGet(ResolveKey(key, regionName)).ToString().ToObject();

		public T Get<T>(string key, string regionName = null)
			=> Database.StringGet(ResolveKey(key, regionName)).ToString().ToObject<T>();

		public async Task<object> GetAsync(string key, string regionName = null)
			=> (await Database.StringGetAsync(ResolveKey(key, regionName))).ToString().ToObject();

		public async Task<T> GetAsync<T>(string key, string regionName = null)
			=> (await Database.StringGetAsync(ResolveKey(key, regionName))).ToString().ToObject<T>();

		public object GetOrAdd(string key, Func<string, object> addFunction, string regionName = null)
		{
			var result = Get(key, regionName);
			if (result == null)
			{
				result = addFunction(key);
				Set(key, result, regionName);
			}

			return result;
		}

		public T GetOrAdd<T>(string key, Func<string, T> addFunction, string regionName = null)
		{
			var result = Get<T>(key, regionName);
			if (result == null)
			{
				result = addFunction(key);
				Set(key, result, regionName);
			}

			return result;
		}

		public async Task<object> GetOrAddAsync(string key, Func<string, object> addFunction, string regionName = null)
		{
			var result = await GetAsync(key, regionName);
			if (result == null)
			{
				result = addFunction(key);
				await SetAsync(key, result, regionName);
			}

			return result;
		}

		public async Task<T> GetOrAddAsync<T>(string key, Func<string, T> addFunction, string regionName = null)
		{
			var result = await GetAsync<T>(key, regionName);
			if (result == null)
			{
				result = addFunction(key);
				await SetAsync(key, result, regionName);
			}

			return result;
		}

		public void Set(string key, object value, string regionName = null)
		{
			if (value != null)
			{
				Database.StringSet(ResolveKey(key, regionName), value.ToJson(), flags: CommandFlags.FireAndForget);
			}
		}

		public async Task SetAsync(string key, object value, string regionName = null)
		{
			if (value != null)
			{
				await Database.StringSetAsync(ResolveKey(key, regionName), value.ToJson(), flags: CommandFlags.FireAndForget);
			}
		}

		private static string ResolveKey(string key, string regionName)
			=> string.IsNullOrEmpty(regionName) ? key : $"{regionName}.{key}";
	}
}
