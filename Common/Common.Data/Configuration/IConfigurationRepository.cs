// ReSharper disable UnusedParameter.Global
namespace CheckListManager.Common.Data.Configuration
{
	public interface IConfigurationRepository : IRepository
	{
		object Get(string key);
		T Get<T>(string key);
	}
}
