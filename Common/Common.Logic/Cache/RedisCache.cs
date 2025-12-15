using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Helpers;
using Newtonsoft.Json;
using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;

namespace CheckListManager.Common.Logic.Cache
{
	public class RedisCache : IRedisCache
	{
		private readonly Lazy<RedLockFactory> _lockFactory;

		private ITransaction _transaction;

		private Lazy<IDatabase> Database { get; }

		public RedisCache(IRedisSetup redisSetup, Lazy<RedLockFactory> lockFactory)
		{
			_lockFactory = lockFactory;
			Database = new Lazy<IDatabase>(() => redisSetup.Connection.Value.GetDatabase());
		}

		public async Task<object> GetAsync(string key, string regionName = null)
			=> (await Database.Value.StringGetAsync(ResolveKey(key, regionName))).ToString().ToObject();

		public async Task<T> GetAsync<T>(string key, string regionName = null)
			=> (await Database.Value.StringGetAsync(ResolveKey(key, regionName))).ToString().ToObject<T>();

		public async Task<T> GetAsync<T>(string key, string regionName = null, params JsonConverter[] converters)
			=> (await Database.Value.StringGetAsync(ResolveKey(key, regionName))).ToString().ToObject<T>(converters);

        public async Task<T> GetWithCompressionAsync<T>(string key, string regionName = null, params JsonConverter[] converters)
        {
            var redisKey = ResolveKey(key, regionName);

            var redisValue = await Database.Value.StringGetAsync(redisKey);
            if (redisValue.IsNullOrEmpty)
                return default;

            var raw = (byte[])redisValue;

            // nuovo formato: "v1|" + gzip bytes
            if (RedisCompressedFormat.TryExtractGZipPayload(raw, out var gzipPayload))
                return CompressionHelper.DeserializeFromCompressedBytes<T>(gzipPayload, useGZipCompression: true, converters);

            // vecchio formato: base64(gzip bytes)
            var base64 = redisValue.ToString();
            var oldGzipBytes = Convert.FromBase64String(base64);

            return CompressionHelper.DeserializeFromCompressedBytes<T>(oldGzipBytes, useGZipCompression: true, converters);
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

		public async Task SetAsync(string key, object value, string regionName = null, TimeSpan? expiry = null)
		{
			if (value != null)
			{
				if (_transaction == null)
					await Database.Value.StringSetAsync(ResolveKey(key, regionName), value.ToJson(), expiry, flags: CommandFlags.FireAndForget);
				else
					await _transaction.StringSetAsync(ResolveKey(key, regionName), value.ToJson(), expiry, flags: CommandFlags.FireAndForget);
			}
		}

		public async Task SetAsync(string key, object value, string regionName = null, TimeSpan? expiry = null, params JsonConverter[] converters)
		{
			if (value != null)
			{
				if (_transaction == null)
					await Database.Value.StringSetAsync(ResolveKey(key, regionName), value.ToJson(converters), expiry, flags: CommandFlags.FireAndForget);
				else
					await _transaction.StringSetAsync(ResolveKey(key, regionName), value.ToJson(converters), expiry, flags: CommandFlags.FireAndForget);
			}
		}

        public async Task SetWithCompressionAsync(string key, object value, string regionName = null, TimeSpan? expiry = null, params JsonConverter[] converters)
        {
            if (value == null) return;

            var redisKey = ResolveKey(key, regionName);

            var gzipBytes = CompressionHelper.SerializeToCompressedBytes(value, useGZipCompression: true, converters);
            var wrapped = RedisCompressedFormat.Wrap(gzipBytes);

            RedisValue redisValue = wrapped;

            if (_transaction == null)
                await Database.Value.StringSetAsync(redisKey, redisValue, expiry, flags: CommandFlags.FireAndForget);
            else
                await _transaction.StringSetAsync(redisKey, redisValue, expiry, flags: CommandFlags.FireAndForget);
        }

        public Task RemoveAsync(string key, string regionName = null)
		{
			return _transaction == null
				? Database.Value.KeyDeleteAsync(ResolveKey(key, regionName), CommandFlags.FireAndForget)
				: _transaction.KeyDeleteAsync(ResolveKey(key, regionName), CommandFlags.FireAndForget);
		}

		public Task<LockObject> LockAsync(string key, string regionName = null, TimeSpan? expiry = null, TimeSpan? wait = null,
			TimeSpan? retry = null, CancellationToken cancellationToken = default)
		{
			if (!expiry.HasValue)
				expiry = TimeSpan.FromSeconds(5); //life time of the lock (it is extended automatically if the program runs)
			if (!wait.HasValue)
				wait = TimeSpan.FromSeconds(10); //waits until is released by previous lock
			if (!retry.HasValue)
				retry = TimeSpan.FromSeconds(1); //time between two attempts

			return _lockFactory.Value.CreateLockAsync(ResolveKey(key, regionName), expiry.Value, wait.Value, retry.Value, cancellationToken)
				.ContinueWith(t => new LockObject(t.Result), cancellationToken);
		}

		public Task<long> IncrementAsync(string key, string regionName = null)
			=> Database.Value.StringIncrementAsync(ResolveKey(key, regionName));

		public void BeginTransaction()
		{
			if (_transaction == null)
				_transaction = Database.Value.CreateTransaction();
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
				return;
			if (_transaction == null)
				await Database.Value.SortedSetRemoveAsync(ResolveKey(key, regionName), value, CommandFlags.FireAndForget);
			else
				await _transaction.SortedSetRemoveAsync(ResolveKey(key, regionName), value, CommandFlags.FireAndForget);
		}

		public Task SortedSetElementUpdateScoreAsync(string key, string value, double score, string regionName = null)
			=> SortedSetElementInsertAsync(key, value, score, regionName, When.Exists);

		private async Task SortedSetElementInsertAsync(string key, string value, double score, string regionName, When when)
		{
			if (string.IsNullOrEmpty(value))
				return;

			if (_transaction == null)
				await Database.Value.SortedSetAddAsync(ResolveKey(key, regionName), value, score, when, CommandFlags.FireAndForget);
			else
				await _transaction.SortedSetAddAsync(ResolveKey(key, regionName), value, score, when, CommandFlags.FireAndForget);
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
