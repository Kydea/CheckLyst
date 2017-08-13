using System;
using System.Runtime.Caching;
using InMemoryCache = System.Runtime.Caching.MemoryCache;

namespace CheckListManager.Common.Core.Cache
{
	public class MemoryCache : IMemoryCache
	{
		private static readonly CacheItemPolicy DefaultCacheItemPolicy = new CacheItemPolicy
		{
			SlidingExpiration = TimeSpan.FromMinutes(60)
		};

		private readonly InMemoryCache _storage = InMemoryCache.Default;

		public object Get(string key, string regionName = null) => _storage.Get(ResolveKey(key, regionName));

		public T Get<T>(string key, string regionName = null) => (T)Get(ResolveKey(key, regionName));

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

		public void Set(string key, object value, string regionName = null)
		{
			if (value != null)
			{
				_storage.Set(ResolveKey(key, regionName), value, DefaultCacheItemPolicy);
			}
		}

		private static string ResolveKey(string key, string regionName)
			=> string.IsNullOrEmpty(regionName) ? key : $"{regionName}.{key}";
	}
}
