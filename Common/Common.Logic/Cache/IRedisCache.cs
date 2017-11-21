using System;
using System.Threading;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Cache;
using Newtonsoft.Json;

namespace CheckListManager.Common.Logic.Cache
{
	public interface IRedisCache : ICache
	{
		Task<T> GetAsync<T>(string key, string regionName = null, params JsonConverter[] converters);
		Task SetAsync(string key, object value, string regionName = null, TimeSpan? expiry = null, params JsonConverter[] converters);

		Task<LockObject> LockAsync(string key, string regionName = null, TimeSpan? expiry = null, TimeSpan? wait = null,
			TimeSpan? retry = null, CancellationToken cancellationToken = default(CancellationToken));

		Task<long> IncrementAsync(string key, string regionName = null);

		void BeginTransaction();
		Task CommitTransactionAsync();
		void RollbackTransaction();

		Task SortedSetElementInsertAsync(string key, string value, double score = 0, string regionName = null);
		Task SortedSetElementRemoveAsync(string key, string value, string regionName = null);
		Task SortedSetElementUpdateScoreAsync(string key, string value, double score = 0, string regionName = null);
		Task<string[]> SortedSetGetAsync(string key, double start = double.NegativeInfinity, double stop = double.PositiveInfinity, string regionName = null);
		Task<long> SortedSetCountAsync(string key, string regionName = null);
		Task<bool> SortedSetContainsAsync(string key, string value, string regionName = null);
	}
}
