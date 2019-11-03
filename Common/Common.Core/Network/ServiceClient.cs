using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CheckListManager.Common.Core.Network
{
	public class ServiceClient : IServiceClient, IDisposable
	{
		private const string AwsTraceHeader = "X-Amzn-Trace-Id";

		private readonly HttpClient _client;
		private bool _configured;

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
				_client?.Dispose();
		}

		protected void Configure(HttpRequestMessage incomingMessage)
		{
			if (incomingMessage == null)
				throw new ArgumentNullException(nameof(incomingMessage), "Incoming Message must have a value");

			if (_configured)
				return;

			_client.DefaultRequestHeaders.Authorization = incomingMessage.Headers.Authorization;
			if (incomingMessage.Headers.TryGetValues(AwsTraceHeader, out var trace))
				_client.DefaultRequestHeaders.TryAddWithoutValidation(AwsTraceHeader, trace.First());
            if (incomingMessage.Headers.TryGetValues("CompanyTimeZone", out var companyTimeZone))
                _client.DefaultRequestHeaders.TryAddWithoutValidation("CompanyTimeZone", companyTimeZone.Single());

            _configured = true;
		}

		public Task<HttpResponseMessage> GetAsync(string url)
			=> _client.GetAsync(url);

		public async Task<T> GetAsync<T>(string url)
		{
			var response = await GetAsync(url);
			return await response.Content.ReadAsAsync<T>();
		}

		public Task<HttpResponseMessage> PostAsync<T>(string url, T payload)
			=> _client.PostAsJsonAsync(url, payload);

		public async Task<TU> PostAsync<T, TU>(string url, T payload)
		{
			var response = await PostAsync(url, payload);
			return await response.Content.ReadAsAsync<TU>();
		}
	}
}
