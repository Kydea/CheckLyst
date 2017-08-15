using CheckListManager.Common.Logic.Services;
using StackExchange.Redis;

namespace CheckListManager.Common.Logic.Cache
{
	public class RedisSetup : IRedisSetup
	{
		public RedisSetup(IConfigurationService configuration)
		{
			Connection = ConnectionMultiplexer.Connect($"{configuration.RedisHost}:{configuration.RedisPort}");
		}

		public IConnectionMultiplexer Connection { get; }
	}
}
