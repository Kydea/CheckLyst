using System;
using Autofac;
using Autofac.Core.Lifetime;
using CheckListManager.Common.Core.Cache;
using CheckListManager.Common.Data.Configuration;

namespace CheckListManager.Common.Logic.Services
{
	public class ConfigurationService : IConfigurationService
	{
		private const string RegionName = "Configuration";

		private readonly ILifetimeScope _scope;
		private readonly IMemoryCache _cache;

		public ConfigurationService(ILifetimeScope scope, IMemoryCache cache)
		{
			_scope = scope;
			_cache = cache;
		}

		public string RedisHost => _cache.GetOrAdd("RedisHost", Load<string>, RegionName);
		public int RedisPort => _cache.GetOrAdd("RedisPort", Load<int>, RegionName);
		public string SqsBaseUrl => _cache.GetOrAdd("SqsBaseUrl", Load<string>, RegionName);
        public string SqsPrefix => _cache.GetOrAdd("SqsPrefix", Load<string>, RegionName);
        public string ApiBaseUrl => _cache.GetOrAdd("ApiBaseUrl", Load<string>, RegionName);
        //public string ApiBaseUrl => "http://localhost/API/";
        public string WebApplicationUrl => _cache.GetOrAdd("WebApplicationUrl", Load<string>, RegionName);
        public string PublicRepositoryUrl => _cache.GetOrAdd("PublicRepositoryUrl", Load<string>, RegionName);
        public int InstanceCloseTimeout => _cache.GetOrAdd("InstanceCloseTimeout", Load<int>, RegionName);
        //public string PrototypesStorageBucket => _cache.GetOrAdd(nameof(PrototypesStorageBucket), Load<string>, RegionName);
        public string PrototypesStorageBucket => "c:\\matematici\\";
        //public string InstancesStorageBucket => _cache.GetOrAdd(nameof(InstancesStorageBucket), Load<string>, RegionName);
        public string InstancesStorageBucket => "c:\\matematici\\";
        public string NotificationsEmailAddress => _cache.GetOrAdd(nameof(NotificationsEmailAddress), Load<string>, RegionName);

        private T Load<T>(string key)
		{
			using (var scope = _scope.BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag))
			{
				var repository = scope.Resolve<IConfigurationRepository>();

				var configuration = repository.Get(key);
				if (configuration == null)
					return default(T);

				var value = GetValue<T>(configuration.Value);
				_cache.Set(key, value, RegionName, TimeSpan.FromMinutes(20));

				return value;
			}
		}

		private static T GetValue<T>(string value)
		{
			var type = typeof(T);
			if (type == typeof(int))
			{
				return int.TryParse(value, out var i) ? (T)(object)i : default(T);
			}
			if (type == typeof(string))
			{
				return (T)(object)value;
			}

			return default(T);
		}
	}
}
