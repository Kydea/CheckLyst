using System.Threading.Tasks;

namespace CheckListManager.Common.Data.Configuration
{
	public interface ISettingRepository : IRepository
	{
		Task<Setting> GetAsync(long companyId, string code);
		Task<TimeZones> GetTimeZoneAsync(long companyId);
	}
}
