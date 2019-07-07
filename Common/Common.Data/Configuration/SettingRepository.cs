using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace CheckListManager.Common.Data.Configuration
{
	public class SettingRepository : Repository, ISettingRepository
	{
		public SettingRepository(RelationalDatabaseEntities entities) : base(entities)
		{
		}

		[SuppressMessage("ReSharper", "ImplicitlyCapturedClosure")]
		public Task<Setting> GetAsync(long companyId, string code)
		{
			var query =
				from s in Entities.Settings
				from v in Entities.SettingValues.Where(x => x.Company == companyId && x.Setting == s.Code).DefaultIfEmpty()
				where
					s.Code == code
				select new
				{
					s.Code,
					s.DefaultValue,
					v.Value
				};

			return query.FirstOrDefaultAsync()
				.ContinueWith(x => new Setting
				{
					Company = companyId,
					Code = x.Result.Code,
					DefaultValue = x.Result.DefaultValue,
					CurrentValue = x.Result.Value ?? x.Result.DefaultValue
				});
		}

        public async Task<TimeZones> GetTimeZoneAsync(long companyId)
            => (await Entities.Companies.SingleAsync(c => c.ID == companyId)).TimeZones;
    }
}