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

        public void Clear(long companyId)
        {
            _cache.Remove($"Companies", "CommonRepository");
            _cache.Remove($"LDAPHost-{companyId}", RegionName);
            _cache.Remove($"SmtpHost-{companyId}", RegionName);
            _cache.Remove($"SmtpPort-{companyId}", RegionName);
            _cache.Remove($"SmtpUsername-{companyId}", RegionName);
            _cache.Remove($"SmtpPassword-{companyId}", RegionName);
            _cache.Remove($"EmailSender-{companyId}", RegionName);
            _cache.Remove($"UserBlockMessage-{companyId}", RegionName);
            _cache.Remove($"UserBlockWarningMessage-{companyId}", RegionName);
            _cache.Remove($"SignatureMessage-{companyId}", RegionName);
            _cache.Remove($"InstanceMessage-{companyId}", RegionName);
            _cache.Remove($"NotificationMessage-{companyId}", RegionName);
            _cache.Remove($"PrototypesFtpConnection-{companyId}", RegionName);
            _cache.Remove($"InstancesFtpConnection-{companyId}", RegionName);
            _cache.Remove($"DecimalSeparator-{companyId}", RegionName);
            _cache.Remove($"TimeZone-{companyId}", RegionName);
            _cache.Remove($"RequireUserReason-{companyId}", RegionName);
            _cache.Remove($"InactivityTimeoutSeconds-{companyId}", RegionName);
            _cache.Remove($"MaximumNumberOfPasswordFailure-{companyId}", RegionName);
            _cache.Remove($"MaximumNumberOfPasswordFailure-{companyId}", RegionName);
            _cache.Remove($"PINDaysLife-{companyId}", RegionName);
            _cache.Remove($"WarningUserInactivityDays-{companyId}", RegionName);
            _cache.Remove($"MaximumUserInactivity-{companyId}", RegionName);
            _cache.Remove($"PrototypesAttachmentMaxSizeBytes-{companyId}", RegionName);
            _cache.Remove($"InstancesAttachmentMaxSizeBytes-{companyId}", RegionName);
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

        public Task<string> NotificationMessageAsync(long companyId)
        {
            const string code = "NotificationMessage";
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

        public Task<string> TimeZoneAsync(long companyId)
        {
            const string code = "TimeZone";
            return _cache.GetOrAddAsync($"{code}-{companyId}", async key => await _settingRepository.GetTimeZoneAsync(companyId), RegionName);
        }

        public Task<bool> RequireUserReasonAsync(long companyId)
        {
	        const string code = "RequireUserReason";
	        return _cache.GetOrAddAsync($"{code}-{companyId}", async key => await Load<bool>(companyId, code), RegionName);
        }

        public Task<int> InactivityTimeoutSecondsAsync(long companyId)
        {
            const string code = "InactivityTimeoutSeconds";
            return _cache.GetOrAddAsync($"{code}-{companyId}", async key => await Load<int>(companyId, code), RegionName);
        }

        public Task<int> MaximumNumberOfPasswordFailureAsync(long companyId)
        {
            const string code = "MaximumNumberOfPasswordFailure";
            return _cache.GetOrAddAsync($"{code}-{companyId}", async key => await Load<int>(companyId, code), RegionName);
        }

        public Task<int> MaximumNumberOfPINFailureAsync(long companyId)
        {
            const string code = "MaximumNumberOfPasswordFailure";
            return _cache.GetOrAddAsync($"{code}-{companyId}", async key => await Load<int>(companyId, code), RegionName);
        }

        public Task<int> PINDaysLifeAsync(long companyId)
        {
            const string code = "PINDaysLife";
            return _cache.GetOrAddAsync($"{code}-{companyId}", async key => await Load<int>(companyId, code), RegionName);
        }

        public Task<int> WarningUserInactivityDaysAsync(long companyId)
        {
            const string code = "WarningUserInactivityDays";
            return _cache.GetOrAddAsync($"{code}-{companyId}", async key => await Load<int>(companyId, code), RegionName);
        }

        public Task<int> MaximumUserInactivityDaysAsync(long companyId)
        {
            const string code = "MaximumUserInactivityDays";
            return _cache.GetOrAddAsync($"{code}-{companyId}", async key => await Load<int>(companyId, code), RegionName);
        }

        public Task<int> PrototypesAttachmentMaxSizeBytesAsync(long companyId)
        {
	        const string code = "PrototypesAttachmentMaxSizeBytes";
	        return _cache.GetOrAddAsync($"{code}-{companyId}", async key => await Load<int>(companyId, code), RegionName);
        }

        public Task<int> InstancesAttachmentMaxSizeBytesAsync(long companyId)
        {
	        const string code = "InstancesAttachmentMaxSizeBytes";
	        return _cache.GetOrAddAsync($"{code}-{companyId}", async key => await Load<int>(companyId, code), RegionName);
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
			if (type == typeof(bool))
				return bool.TryParse(value, out var i) ? (T)(object)i : default(T);
			if (type == typeof(string))
				return (T)(object)value;

			return default(T);
		}
	}
}