using System;
using StackExchange.Redis;

namespace CheckListManager.Common.Logic.Cache
{
	public interface IRedisSetup
	{
		Lazy<IConnectionMultiplexer> Connection { get; }
	}
}
