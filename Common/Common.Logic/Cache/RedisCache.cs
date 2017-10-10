using System;
using System.Threading;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Helpers;
using RedLock;
using StackExchange.Redis;

namespace CheckListManager.Common.Logic.Cache
{
	public class RedisCache : IRedisCache
	{
		private readonly Lazy<IRedisLockFactory> _lockFactory;

		private Lazy<IDatabase> Database { get; }

		public RedisCache(IRedisSetup redisSetup, Lazy<IRedisLockFactory> lockFactory)
		{
			_lockFactory = lockFactory;
			Database = new Lazy<IDatabase>(() => redisSetup.Connection.Value.GetDatabase());
		}

		public object Get(string key, string regionName = null)
			=> Database.Value.StringGet(ResolveKey(key, regionName)).ToString().ToObject();

		public T Get<T>(string key, string regionName = null)
			=> Database.Value.StringGet(ResolveKey(key, regionName)).ToString().ToObject<T>();

		public async Task<object> GetAsync(string key, string regionName = null)
			=> (await Database.Value.StringGetAsync(ResolveKey(key, regionName))).ToString().ToObject();

		public async Task<T> GetAsync<T>(string key, string regionName = null)
			=> (await Database.Value.StringGetAsync(ResolveKey(key, regionName))).ToString().ToObject<T>();

		public object GetOrAdd(string key, Func<string, object> addFunction, string regionName = null, TimeSpan? expiry = null)
		{
			var result = Get(key, regionName);
			if (result == null)
			{
				result = addFunction(key);
				Set(key, result, regionName, expiry);
			}

			return result;
		}

		public T GetOrAdd<T>(string key, Func<string, T> addFunction, string regionName = null, TimeSpan? expiry = null)
		{
			var result = Get<T>(key, regionName);
			if (result == null)
			{
				result = addFunction(key);
				Set(key, result, regionName, expiry);
			}

			return result;
		}

		public async Task<object> GetOrAddAsync(string key, Func<string, object> addFunction, string regionName = null, TimeSpan? expiry = null)
		{
			var result = await GetAsync(key, regionName);
			if (result == null)
			{
				result = addFunction(key);
				await SetAsync(key, result, regionName, expiry);
			}

			return result;
		}

		public async Task<T> GetOrAddAsync<T>(string key, Func<string, T> addFunction, string regionName = null, TimeSpan? expiry = null)
		{
			var result = await GetAsync<T>(key, regionName);
			if (result == null)
			{
				result = addFunction(key);
				await SetAsync(key, result, regionName, expiry);
			}

			return result;
		}

		public async Task<T> GetOrAddAsync<T>(string key, Func<string, Task<T>> addFunctionAsync, string regionName = null, TimeSpan? expiry = null)
		{
			var result = await GetAsync<T>(key, regionName);
			if (result == null)
			{
				result = await addFunctionAsync(key);
				await SetAsync(key, result, regionName, expiry);
			}

			return result;
		}

		public void Set(string key, object value, string regionName = null, TimeSpan? expiry = null)
		{
			if (value != null)
			{
				Database.Value.StringSet(ResolveKey(key, regionName), value.ToJson(), expiry, flags: CommandFlags.FireAndForget);
			}
		}

		public async Task SetAsync(string key, object value, string regionName = null, TimeSpan? expiry = null)
		{
			if (value != null)
			{
				await Database.Value.StringSetAsync(ResolveKey(key, regionName), value.ToJson(), expiry, flags: CommandFlags.FireAndForget);
			}
		}

		public void Remove(string key, string regionName = null)
		{
			Database.Value.KeyDelete(ResolveKey(key, regionName), CommandFlags.FireAndForget);
		}

		public Task RemoveAsync(string key, string regionName = null)
		{
			return Database.Value.KeyDeleteAsync(ResolveKey(key, regionName), CommandFlags.FireAndForget);
		}

		public Task<LockObject> LockAsync(string key, string regionName = null, TimeSpan? expiry = null, TimeSpan? wait = null,
			TimeSpan? retry = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (!expiry.HasValue)
			{
				expiry = TimeSpan.FromSeconds(5);
			}
			if (!wait.HasValue)
			{
				wait = TimeSpan.FromSeconds(10);
			}
			if (!retry.HasValue)
			{
				retry = TimeSpan.FromSeconds(1);
			}

			return _lockFactory.Value.CreateAsync(ResolveKey(key, regionName), expiry.Value, wait.Value, retry.Value, cancellationToken)
				.ContinueWith(t => new LockObject(t.Result), cancellationToken);
		}

		private static string ResolveKey(string key, string regionName)
			=> string.IsNullOrEmpty(regionName) ? key : $"{regionName}.{key}";
	}
}
