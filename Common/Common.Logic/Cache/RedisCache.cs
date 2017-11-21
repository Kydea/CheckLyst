using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Helpers;
using Newtonsoft.Json;
using RedLock;
using StackExchange.Redis;

namespace CheckListManager.Common.Logic.Cache
{
	public class RedisCache : IRedisCache
	{
		private readonly Lazy<IRedisLockFactory> _lockFactory;

		private ITransaction _transaction;

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

		public async Task<T> GetAsync<T>(string key, string regionName = null, params JsonConverter[] converters)
			=> (await Database.Value.StringGetAsync(ResolveKey(key, regionName))).ToString().ToObject<T>(converters);

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
				if (_transaction == null)
				{
					await Database.Value.StringSetAsync(ResolveKey(key, regionName), value.ToJson(), expiry, flags: CommandFlags.FireAndForget);
				}
				else
				{
					await _transaction.StringSetAsync(ResolveKey(key, regionName), value.ToJson(), expiry, flags: CommandFlags.FireAndForget);
				}
			}
		}

		public async Task SetAsync(string key, object value, string regionName = null, TimeSpan? expiry = null, params JsonConverter[] converters)
		{
			if (value != null)
			{
				if (_transaction == null)
				{
					await Database.Value.StringSetAsync(ResolveKey(key, regionName), value.ToJson(converters), expiry, flags: CommandFlags.FireAndForget);
				}
				else
				{
					await _transaction.StringSetAsync(ResolveKey(key, regionName), value.ToJson(converters), expiry, flags: CommandFlags.FireAndForget);
				}
			}
		}

		public void Remove(string key, string regionName = null)
		{
			Database.Value.KeyDelete(ResolveKey(key, regionName), CommandFlags.FireAndForget);
		}

		public Task RemoveAsync(string key, string regionName = null)
		{
			return _transaction == null
				? Database.Value.KeyDeleteAsync(ResolveKey(key, regionName), CommandFlags.FireAndForget)
				: _transaction.KeyDeleteAsync(ResolveKey(key, regionName), CommandFlags.FireAndForget);
		}

		public Task<LockObject> LockAsync(string key, string regionName = null, TimeSpan? expiry = null, TimeSpan? wait = null,
			TimeSpan? retry = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (!expiry.HasValue)
			{
				expiry = TimeSpan.FromSeconds(5); //life time of the lock (it is extended automatically if the program runs)
			}
			if (!wait.HasValue)
			{
				wait = TimeSpan.FromSeconds(10); //waits until is released by previous lock
			}
			if (!retry.HasValue)
			{
				retry = TimeSpan.FromSeconds(1); //time between two attempts
            }

			return _lockFactory.Value.CreateAsync(ResolveKey(key, regionName), expiry.Value, wait.Value, retry.Value, cancellationToken)
				.ContinueWith(t => new LockObject(t.Result), cancellationToken);
		}

		public Task<long> IncrementAsync(string key, string regionName = null)
			=> Database.Value.StringIncrementAsync(ResolveKey(key, regionName));

		public void BeginTransaction()
		{
			if (_transaction == null)
			{
				_transaction = Database.Value.CreateTransaction();
			}
		}

		public async Task CommitTransactionAsync()
		{
			if (_transaction != null)
			{
				await _transaction.ExecuteAsync();
				_transaction = null;
			}
		}

		public void RollbackTransaction()
		{
			_transaction = null;
		}

		public Task SortedSetElementInsertAsync(string key, string value, double score, string regionName = null)
			=> SortedSetElementInsertAsync(key, value, score, regionName, When.Always);

		public async Task SortedSetElementRemoveAsync(string key, string value, string regionName = null)
		{
			if (string.IsNullOrEmpty(value))
			{
				return;
			}
			if (_transaction == null)
			{
				await Database.Value.SortedSetRemoveAsync(ResolveKey(key, regionName), value, CommandFlags.FireAndForget);
			}
			else
			{
				await _transaction.SortedSetRemoveAsync(ResolveKey(key, regionName), value, CommandFlags.FireAndForget);
			}
		}

		public Task SortedSetElementUpdateScoreAsync(string key, string value, double score, string regionName = null)
			=> SortedSetElementInsertAsync(key, value, score, regionName, When.Exists);

		private async Task SortedSetElementInsertAsync(string key, string value, double score, string regionName, When when)
		{
			if (string.IsNullOrEmpty(value))
			{
				return;
			}

			if (_transaction == null)
			{
				await Database.Value.SortedSetAddAsync(ResolveKey(key, regionName), value, score, when, CommandFlags.FireAndForget);
			}
			else
			{
				await _transaction.SortedSetAddAsync(ResolveKey(key, regionName), value, score, when, CommandFlags.FireAndForget);
			}
		}

		public async Task<string[]> SortedSetGetAsync(string key, double start = double.NegativeInfinity, double stop = double.PositiveInfinity,
			string regionName = null)
			=> await Database.Value.SortedSetRangeByScoreAsync(ResolveKey(key, regionName), start, stop)
				.ContinueWith(r => r.Result.Select(x => x.ToString()).ToArray());

		public Task<long> SortedSetCountAsync(string key, string regionName = null)
			=> Database.Value.SortedSetLengthAsync(ResolveKey(key, regionName));

		public Task<bool> SortedSetContainsAsync(string key, string value, string regionName = null)
			=> Database.Value.SortedSetLengthByValueAsync(ResolveKey(key, regionName), value, value)
				.ContinueWith(r => r.Result > 0);

		private static string ResolveKey(string key, string regionName)
			=> string.IsNullOrEmpty(regionName) ? key : $"{regionName}:{key}";
	}
}
