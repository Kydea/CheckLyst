using System;
using System.Threading;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Cache;

namespace CheckListManager.Common.Logic.Cache
{
	public interface IRedisCache : ICache
	{
		Task<LockObject> LockAsync(string key, string regionName = null, TimeSpan? expiry = null, TimeSpan? wait = null,
			TimeSpan? retry = null, CancellationToken cancellationToken = default(CancellationToken));
	}
}
