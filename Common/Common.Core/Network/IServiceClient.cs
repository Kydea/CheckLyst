using System.Net.Http;
using System.Threading.Tasks;

namespace CheckListManager.Common.Core.Network
{
	public interface IServiceClient
	{
		Task<HttpResponseMessage> Get(string url);
		Task<T> Get<T>(string url);

		Task<HttpResponseMessage> Post<T>(string url, T payload);
		Task<TU> Post<T, TU>(string url, T payload);
	}
}