using System.Net.Http;

namespace CheckListManager.Common.Core.Network
{
	public interface IServiceClient
	{
		HttpResponseMessage Get(string url);
		T Get<T>(string url);

		HttpResponseMessage Post<T>(string url, T payload);
		TU Post<T, TU>(string url, T payload);
	}
}