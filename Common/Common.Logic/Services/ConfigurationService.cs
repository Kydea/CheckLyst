using CheckListManager.Common.Core.Cache;

namespace CheckListManager.Common.Logic.Services
{
	public class ConfigurationService : IConfigurationService
	{
		//private readonly IConfigurationRepository _repository;
		private readonly IMemoryCache _cache;

		public ConfigurationService(/*IConfigurationRepository repository, */IMemoryCache cache)
		{
			//_repository = repository;
			_cache = cache;
		}

		public string RedisHost => _cache.GetOrAdd("RedisHost", key => "redis-01.clm.internal" /* USE Repository */, "Configuration");

		public int RedisPort => _cache.GetOrAdd("RedisPort", key => 6379 /* USE Repository */, "Configuration");
	}
}
