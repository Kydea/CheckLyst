using System.Net.Http;

namespace CheckListManager.Common.Core.Web.Network
{
	public class ServiceClient : Core.Network.ServiceClient
	{
		public ServiceClient(string baseUrl, HttpRequestMessage incomingMessage) : base(baseUrl)
		{
			Configure(incomingMessage);
		}
	}
}
