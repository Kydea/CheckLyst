using System;
using System.Threading.Tasks;

// ReSharper disable UnusedMemberInSuper.Global

namespace CheckListManager.Common.Core.Cache
{
	public interface ICache
	{
		Task<object> GetAsync(string key, string regionName = null);
		Task<T> GetAsync<T>(string key, string regionName = null);

		Task<object> GetOrAddAsync(string key, Func<string, object> addFunction, string regionName = null, TimeSpan? expiry = null);
		Task<T> GetOrAddAsync<T>(string key, Func<string, T> addFunction, string regionName = null, TimeSpan? expiry = null);
		Task<T> GetOrAddAsync<T>(string key, Func<string, Task<T>> addFunctionAsync, string regionName = null, TimeSpan? expiry = null);

		Task SetAsync(string key, object value, string regionName = null, TimeSpan? expiry = null);

		Task RemoveAsync(string key, string regionName = null);
	}
}
