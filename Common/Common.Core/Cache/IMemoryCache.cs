using System;

namespace CheckListManager.Common.Core.Cache
{
	public interface IMemoryCache : ICache
	{
		T GetOrAdd<T>(string key, Func<string, T> addFunction, string regionName = null, TimeSpan? expiry = null);
		void Set(string key, object value, string regionName = null, TimeSpan? expiry = null);
	}
}
