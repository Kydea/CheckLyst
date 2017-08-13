using System;

namespace CheckListManager.Common.Core.Cache
{
	public interface ICache
	{
		object Get(string key, string regionName = null);
		T Get<T>(string key, string regionName = null);

		object GetOrAdd(string key, Func<string, object> addFunction, string regionName = null);
		T GetOrAdd<T>(string key, Func<string, T> addFunction, string regionName = null);

		void Set(string key, object value, string regionName = null); //TODO: add overlow to pass CacheItemPolicy
	}
}
