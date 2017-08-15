using StackExchange.Redis;

namespace CheckListManager.Common.Logic.Cache
{
	public interface IRedisSetup
	{
		IConnectionMultiplexer Connection { get; }
	}
}
