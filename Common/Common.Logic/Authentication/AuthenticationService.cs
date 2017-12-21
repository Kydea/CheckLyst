using System;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CheckListManager.Common.Data;
using CheckListManager.Common.Core.Network;
using Newtonsoft.Json;

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
            try
            {
                var httpResponse = await _client.PostAsync("AccessControl/Access/Authenticate", new
                {
                    CompanyId = companyId,
                    Code = code,
                    Password = password,
                });
                var responseData = await httpResponse.Content.ReadAsStringAsync();

                //var response = JsonConvert.DeserializeObject(responseData, _converter);

                //return response;
            }
            catch (Exception e)
            {
                throw new Exception("Authentication failed", e);
            }

        }
    }
}
