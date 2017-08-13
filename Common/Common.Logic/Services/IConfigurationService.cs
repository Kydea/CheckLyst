using CheckListManager.Common.Core.Services;

namespace CheckListManager.Common.Logic.Services
{
	public interface IConfigurationService : IService
	{
		string RedisHost { get; }
		int RedisPort { get; }
	}
}
