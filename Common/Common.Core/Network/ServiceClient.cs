using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CheckListManager.Common.Core.Network
{
	public class ServiceClient : IServiceClient, IDisposable
	{
		private readonly HttpClient _client;

		public ServiceClient(string baseUrl)
		{
			_client = new HttpClient { BaseAddress = new Uri(baseUrl) };
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		// ReSharper disable once VirtualMemberNeverOverridden.Global
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				_client?.Dispose();
			}
		}

		public Task<HttpResponseMessage> Get(string url)
			=> _client.GetAsync(url);

		public async Task<T> Get<T>(string url)
		{
			var response = await Get(url);
			return await response.Content.ReadAsAsync<T>();
		}

		public Task<HttpResponseMessage> Post<T>(string url, T payload)
			=> _client.PostAsJsonAsync(url, payload);

		public async Task<TU> Post<T, TU>(string url, T payload)
		{
			var response = await Post(url, payload);
			return await response.Content.ReadAsAsync<TU>();
		}
	}
}
