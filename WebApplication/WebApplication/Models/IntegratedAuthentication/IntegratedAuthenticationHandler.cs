using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;

namespace CheckListManager.WebApplication.Models.IntegratedAuthentication
{
    public class IntegratedAuthenticationHandler : AuthenticationHandler<IntegratedAuthenticationOptions>
    {
        public IntegratedAuthenticationHandler(IntegratedAuthenticationService service)
        {
            Service = service ?? throw new ArgumentNullException();
        }

        private IntegratedAuthenticationService Service { get; }

        protected override async Task<AuthenticationTicket> AuthenticateCoreAsync()
        {
            if (await Task.Run(() => IsAuthorized(Request.Headers)))
            {
                AuthenticationProperties properties = new AuthenticationProperties
                {
                    IssuedUtc = DateTime.UtcNow,
                    ExpiresUtc = DateTime.UtcNow.AddDays(1),
                    AllowRefresh = true,
                    IsPersistent = true
                };
                IList<Claim> claimCollection = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, "Massimiliano Carosi"),
                    new Claim(ClaimTypes.Role, "Administrator"),
                    new Claim("Company", "Janssen")
                };
                ClaimsIdentity claimsIdentity = new ClaimsIdentity(claimCollection, "Custom");
                AuthenticationTicket ticket = new AuthenticationTicket(claimsIdentity, properties);
                return ticket;
            }
    
            return null;
        }

        private bool IsAuthorized(IHeaderDictionary requestHeaders)
        {
            return Service.IsAuthorised(requestHeaders);
        }
    }
}
