using System;
using System.Threading.Tasks;
// ReSharper disable UnusedMemberInSuper.Global

namespace CheckListManager.Common.Core.Cache
{
	public interface ICache
	{
		object Get(string key, string regionName = null);
		T Get<T>(string key, string regionName = null);
		Task<object> GetAsync(string key, string regionName = null);
		Task<T> GetAsync<T>(string key, string regionName = null);

		object GetOrAdd(string key, Func<string, object> addFunction, string regionName = null);
		T GetOrAdd<T>(string key, Func<string, T> addFunction, string regionName = null);
		Task<object> GetOrAddAsync(string key, Func<string, object> addFunction, string regionName = null);
		Task<T> GetOrAddAsync<T>(string key, Func<string, T> addFunction, string regionName = null);

		void Set(string key, object value, string regionName = null);
		Task SetAsync(string key, object value, string regionName = null);

		void Remove(string key, string regionName = null);
		Task RemoveAsync(string key, string regionName = null);
	}
}
