namespace CheckListManager.Common.Data.Configuration
{
	public class ConfigurationRepository : Repository, IConfigurationRepository
	{
		public ConfigurationRepository(RelationalDatabaseEntities entities) : base(entities)
		{
		}

		public object Get(string key)
		{
			//TODO: read from DB
			return null;
		}

		public T Get<T>(string key)
		{
			//TODO: read from DB
			return default(T);
		}
	}
}