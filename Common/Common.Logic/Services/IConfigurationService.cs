using CheckListManager.Common.Core.Services;

namespace CheckListManager.Common.Logic.Services
{
	public interface IConfigurationService : ISingletonService
	{
		string RedisHost { get; }
		int RedisPort { get; }

		string SqsBaseUrl { get; }
	}
}
