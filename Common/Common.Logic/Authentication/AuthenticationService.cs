using System;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CheckListManager.Common.Data;
using CheckListManager.Common.Core.Network;
using Newtonsoft.Json;
using System.Net.Http;

namespace CheckListManager.Common.Logic.Authentication
{
	public class AuthenticationService : IAuthenticationService
    {
        private IServiceClient _client;

        public AuthenticationService(IServiceClient client)
        {
            _client = client;
        }

        public async Task AuthenticateAsync(long? companyId, string code, string password)
        {
            HttpResponseMessage response;
            string responseData;
            try
            {
                response = await _client.PostAsync("AccessControl/Access/Authenticate", new
                {
                    CompanyId = companyId,
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
                throw new Exception(JsonConvert.DeserializeObject<AuthorizeResponse>(responseData).Message);
        }
    }
}
