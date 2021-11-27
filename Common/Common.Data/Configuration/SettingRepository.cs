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
		public async Task<Setting> GetAsync(long companyId, string code)
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

			var row = await query.FirstOrDefaultAsync();
			return row == null ? null : new Setting
			{
				Company = companyId,
				Code = row.Code,
				DefaultValue = row.DefaultValue,
				CurrentValue = row.Value ?? row.DefaultValue
			};
		}
		
        public async Task<string> GetTimeZoneAsync(long companyId)
            => (await Entities.Companies.SingleAsync(c => c.ID == companyId)).TimeZone;
    }
}