using System.Net.Http;
using System.Threading.Tasks;

namespace CheckListManager.Common.Core.Network
{
	public interface IServiceClient
	{
		Task<HttpResponseMessage> GetAsync(string url);
		Task<T> GetAsync<T>(string url);

		Task<HttpResponseMessage> PostAsync<T>(string url, T payload);
		Task<TU> PostAsync<T, TU>(string url, T payload);
	}
}