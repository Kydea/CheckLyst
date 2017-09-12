using System;
using CheckListManager.Common.Logic.Services;
using StackExchange.Redis;

namespace CheckListManager.Common.Logic.Cache
{
	public class RedisSetup : IRedisSetup
	{
		public RedisSetup(IConfigurationService configuration)
		{
			Connection = new Lazy<IConnectionMultiplexer>(() => ConnectionMultiplexer.Connect($"{configuration.RedisHost}:{configuration.RedisPort}"));
		}

		public Lazy<IConnectionMultiplexer> Connection { get; }
	}
}
