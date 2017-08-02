using Common.Core.Cache;
using StackExchange.Redis;

namespace Common.Logic.Cache
{
	public class RedisCache : ICache
	{
		protected IDatabase Database { get; }

		public RedisCache(IConnectionMultiplexer connectionMultiplexer)
		{
			Database = connectionMultiplexer.GetDatabase();
		}


	}
}
