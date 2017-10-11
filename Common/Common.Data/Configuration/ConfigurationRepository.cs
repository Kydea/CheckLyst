using System.Linq;

namespace CheckListManager.Common.Data.Configuration
{
	public class ConfigurationRepository : Repository, IConfigurationRepository
	{
		public ConfigurationRepository(RelationalDatabaseEntities entities) : base(entities)
		{
		}

		public Configurations Get(string key)
			=> Entities.Configurations.FirstOrDefault(c => c.Code == key);
	}
}