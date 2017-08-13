using CheckListManager.Common.Core.Cache;
using CheckListManager.Common.Data.Configuration;

namespace CheckListManager.Common.Logic.Services
{
	public class ConfigurationService : ServiceBase, IConfigurationService
	{
		private readonly IConfigurationRepository _repository;
		private readonly IMemoryCache _cache;

		public ConfigurationService(IConfigurationRepository repository, IMemoryCache cache)
		{
			_repository = repository;
			_cache = cache;
		}

		public string RedisHost => _cache.GetOrAdd("RedisHost", key => "default host" /* USE Repository */, "Configuration");

		public int RedisPort => _cache.GetOrAdd("RedisPort", key => 6379 /* USE Repository */, "Configuration");
	}
}
