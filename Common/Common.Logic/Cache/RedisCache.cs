using System;
using StackExchange.Redis;

namespace CheckListManager.Common.Logic.Cache
{
	public class RedisCache : IRedisCache
	{
		protected IDatabase Database { get; }

		public RedisCache(IConnectionMultiplexer connectionMultiplexer)
		{
			Database = connectionMultiplexer.GetDatabase();
		}

		public object Get(string key, string regionName = null)
		{
			throw new NotImplementedException();
		}

		public T Get<T>(string key, string regionName = null)
		{
			throw new NotImplementedException();
		}

		public object GetOrAdd(string key, Func<string, object> addFunction, string regionName = null)
		{
			throw new NotImplementedException();
		}

		public T GetOrAdd<T>(string key, Func<string, T> addFunction, string regionName = null)
		{
			throw new NotImplementedException();
		}

		public void Set(string key, object value, string regionName = null)
		{
			throw new NotImplementedException();
		}
	}
}
