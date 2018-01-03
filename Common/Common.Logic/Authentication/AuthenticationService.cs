using System;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Network;
using Newtonsoft.Json;
using System.Net.Http;

namespace CheckListManager.Common.Logic.Authentication
{
	public class AuthenticationService : IAuthenticationService
    {
        private readonly IServiceClient _client;

        public AuthenticationService(IServiceClient client)
        {
            _client = client;
        }

        public async Task AuthenticateAsync(string code, string password)
        {
            HttpResponseMessage response;
            string responseData;
            try
            {
                response = await _client.PostAsync("AccessControl/Access/Authenticate", new
                {
                    Code = code,
                    Password = password,
                });
                responseData = await response.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                throw new Exception("Authentication failed", e);
            }

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                throw new Exception(string.Join(" ", JsonConvert.DeserializeObject<AuthenticateResponse>(responseData).Messages));
        }

        public async Task AuthenticateWithPINAsync(string pin)
        {
            HttpResponseMessage response;
            string responseData;
            try
            {
                response = await _client.PostAsync("AccessControl/Access/AuthenticateWithPIN", new
                {
                    PIN = pin,
                });
                responseData = await response.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                throw new Exception("Authentication failed", e);
            }

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                throw new Exception(string.Join(" ", JsonConvert.DeserializeObject<AuthenticateResponse>(responseData).Messages));
        }
    }
}
