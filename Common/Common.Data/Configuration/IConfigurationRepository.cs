namespace CheckListManager.Common.Data.Configuration
{
	public interface IConfigurationRepository : IRepository
	{
		Configurations Get(string key);
	}
}
