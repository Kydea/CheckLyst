using System;
using System.Runtime.Caching;
using System.Threading.Tasks;
using InMemoryCache = System.Runtime.Caching.MemoryCache;

namespace CheckListManager.Common.Core.Cache
{
	public class MemoryCache : IMemoryCache
	{
		private static readonly CacheItemPolicy DefaultCacheItemPolicy = new CacheItemPolicy
		{
			AbsoluteExpiration = DateTime.MaxValue
		};

		private readonly InMemoryCache _storage = InMemoryCache.Default;

		public object Get(string key, string regionName = null) => _storage.Get(ResolveKey(key, regionName));

		public T Get<T>(string key, string regionName = null)
		{
			var result = Get(ResolveKey(key, regionName));
			// ReSharper disable once MergeConditionalExpression
			return result != null
				? (T)result
				: default(T);
		}
		public Task<object> GetAsync(string key, string regionName = null) => Task.FromResult(Get(ResolveKey(key, regionName)));

		public Task<T> GetAsync<T>(string key, string regionName = null) => Task.FromResult(Get<T>(ResolveKey(key, regionName)));

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
			if (Equals(result, default(T)))
			{
				result = addFunction(key);
				Set(key, result, regionName, expiry);
			}

			return result;
		}

		public Task<object> GetOrAddAsync(string key, Func<string, object> addFunction, string regionName = null, TimeSpan? expiry = null)
			=> Task.FromResult(GetOrAdd(key, addFunction, regionName, expiry));

		public Task<T> GetOrAddAsync<T>(string key, Func<string, T> addFunction, string regionName = null, TimeSpan? expiry = null)
			=> Task.FromResult(GetOrAdd(key, addFunction, regionName, expiry));

		public async Task<T> GetOrAddAsync<T>(string key, Func<string, Task<T>> addFunctionAsync, string regionName = null, TimeSpan? expiry = null)
		{
			var result = Get<T>(key, regionName);
			if (Equals(result, default(T)))
			{
				result = await addFunctionAsync(key);
				Set(key, result, regionName, expiry);
			}

			return result;
		}

		public void Set(string key, object value, string regionName = null, TimeSpan? expiry = null)
		{
			if (value != null)
				_storage.Set(ResolveKey(key, regionName), value,
					expiry == null
						? DefaultCacheItemPolicy
						: new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.Add(expiry.Value) });
		}

		public async Task SetAsync(string key, object value, string regionName = null, TimeSpan? expiry = null)
		{
			await Task.Run(() => Set(key, value, regionName, expiry));
		}

		public void Remove(string key, string regionName = null)
		{
			_storage.Remove(ResolveKey(key, regionName));
		}

		public async Task RemoveAsync(string key, string regionName = null)
		{
			await Task.Run(() => Remove(key, regionName));
		}

		private static string ResolveKey(string key, string regionName)
			=> string.IsNullOrEmpty(regionName) ? key : $"{regionName}.{key}";
	}
}
