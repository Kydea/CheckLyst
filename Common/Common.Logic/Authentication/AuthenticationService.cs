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
                throw new Exception(string.Join(" ", JsonConvert.DeserializeObject<AuthenticateResponse>(responseData).Messages));
        }

        public async Task AuthenticateWithPINAsync(long? companyId, string token, string pin)
        {
            HttpResponseMessage response;
            string responseData;
            try
            {
                //TODO creare un overload di PostAsync che preveda il token come parametro, creare il metodo AuthenticateWithPIN che controlla il pin dell'identity del token, etc...
                //OPPURE!!!: forziamo l'utente del lock, non il contrario, non c'è un problema di sicurezza, e usimamo default identity
                response = await _client.PostAsync("AccessControl/Access/AuthenticateWithPIN", new
                {
                    CompanyId = companyId,
                    Token = token,
                    PIN = pin,
                });
                responseData = await response.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                throw new Exception("Authentication failed", e);
            }

            if (true || response.StatusCode != System.Net.HttpStatusCode.OK) //TODO
                throw new Exception(string.Join(" ", JsonConvert.DeserializeObject<AuthenticateResponse>(responseData).Messages));
        }
    }
}
