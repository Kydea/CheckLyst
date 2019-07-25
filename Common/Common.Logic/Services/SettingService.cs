using System;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Cache;
using CheckListManager.Common.Data;
using CheckListManager.Common.Data.Configuration;

namespace CheckListManager.Common.Logic.Services
{
	public class SettingService : ISettingService
	{
		private const string RegionName = "Setting";

		private readonly ISettingRepository _settingRepository;
		private readonly IMemoryCache _cache;

		public SettingService(ISettingRepository settingRepository, IMemoryCache cache)
		{
			_settingRepository = settingRepository;
			_cache = cache;
		}

        public Task<string> LDAPHostAsync(long companyId)
        {
            const string code = "LDAPHost";
            return _cache.GetOrAddAsync($"{code}-{companyId}", async key => await Load<string>(companyId, code), RegionName);
        }


        public Task<string> SmtpHostAsync(long companyId)
		{
			const string code = "SmtpHost";
			return _cache.GetOrAddAsync($"{code}-{companyId}", async key => await Load<string>(companyId, code), RegionName);
		}

		public Task<int> SmtpPortAsync(long companyId)
		{
			const string code = "SmtpPort";
			return _cache.GetOrAddAsync($"{code}-{companyId}", async key => await Load<int>(companyId, code), RegionName);
		}

		public Task<string> SmtpUsernameAsync(long companyId)
		{
			const string code = "SmtpUsername";
			return _cache.GetOrAddAsync($"{code}-{companyId}", async key => await Load<string>(companyId, code), RegionName);
		}

		public Task<string> SmtpPasswordAsync(long companyId)
		{
			const string code = "SmtpPassword";
			return _cache.GetOrAddAsync($"{code}-{companyId}", async key => await Load<string>(companyId, code), RegionName);
		}

		public Task<string> EmailSenderAsync(long companyId)
		{
			const string code = "EmailSender";
			return _cache.GetOrAddAsync($"{code}-{companyId}", async key => await Load<string>(companyId, code), RegionName);
		}

		public Task<string> UserBlockMessageAsync(long companyId)
		{
			const string code = "UserBlockMessage";
			return _cache.GetOrAddAsync($"{code}-{companyId}", async key => await Load<string>(companyId, code), RegionName);
		}

		public Task<string> UserBlockWarningMessageAsync(long companyId)
		{
			const string code = "UserBlockWarningMessage";
			return _cache.GetOrAddAsync($"{code}-{companyId}", async key => await Load<string>(companyId, code), RegionName);
		}

		public Task<string> SignatureMessageAsync(long companyId)
		{
			const string code = "SignatureMessage";
			return _cache.GetOrAddAsync($"{code}-{companyId}", async key => await Load<string>(companyId, code), RegionName);
		}

        public Task<string> InstanceMessageAsync(long companyId)
        {
            const string code = "InstanceMessage";
            return _cache.GetOrAddAsync($"{code}-{companyId}", async key => await Load<string>(companyId, code), RegionName);
        }

        public Task<string> PrototypesFtpConnectionAsync(long companyId)
        {
            const string code = "PrototypesFtpConnection";
            return _cache.GetOrAddAsync($"{code}-{companyId}", async key => await Load<string>(companyId, code), RegionName);
        }

        public Task<string> InstancesFtpConnectionAsync(long companyId)
        {
            const string code = "InstancesFtpConnection";
            return _cache.GetOrAddAsync($"{code}-{companyId}", async key => await Load<string>(companyId, code), RegionName);
        }

        public Task<string> DecimalSeparatorAsync(long companyId)
        {
            const string code = "DecimalSeparator";
            return _cache.GetOrAddAsync($"{code}-{companyId}", async key => await Load<string>(companyId, code), RegionName);
        }

        public Task<TimeZones> TimeZoneAsync(long companyId)
        {
            const string code = "TimeZone";
            return _cache.GetOrAddAsync($"{code}-{companyId}", async key => await _settingRepository.GetTimeZoneAsync(companyId), RegionName);
        }

        private async Task<T> Load<T>(long companyId, string code)
		{
			var configuration = await _settingRepository.GetAsync(companyId, code);
			if (configuration == null)
				return default(T);

			var value = GetValue<T>(configuration.CurrentValue);
			return value;
		}

		private static T GetValue<T>(string value)
		{
			var type = typeof(T);
			if (type == typeof(int))
				return int.TryParse(value, out var i) ? (T)(object)i : default(T);
			if (type == typeof(string))
				return (T)(object)value;

			return default(T);
		}
	}
}