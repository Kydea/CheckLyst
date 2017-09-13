using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace WebApplication.Models
{
    public class IntegratedAuthenticationHandler : AuthenticationHandler<IntegratedAuthenticationOptions>
    {
        public IntegratedAuthenticationHandler(IntegratedAuthenticationService Service)
        {
            this.Service = Service ?? throw new ArgumentNullException();
        }

        private IntegratedAuthenticationService Service { get; }

        protected override async Task<AuthenticationTicket> AuthenticateCoreAsync()
        {
            if (await Task.Run(() => IsAuthorized(Request.Headers)))
            {
                AuthenticationProperties Properties = new AuthenticationProperties
                {
                    IssuedUtc = DateTime.UtcNow,
                    ExpiresUtc = DateTime.UtcNow.AddDays(1),
                    AllowRefresh = true,
                    IsPersistent = true
                };
                IList<Claim> ClaimCollection = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, "Massimiliano Carosi"),
                    new Claim(ClaimTypes.Role, "Administrator"),
                    new Claim("Company", "Janssen")
                };
                ClaimsIdentity ClaimsIdentity = new ClaimsIdentity(ClaimCollection, "Custom");
                AuthenticationTicket Ticket = new AuthenticationTicket(ClaimsIdentity, Properties);
                return Ticket;
            }
    
            return null;
        }

        private bool IsAuthorized(IHeaderDictionary RequestHeaders)
        {
            return Service.IsAuthorised(RequestHeaders);
        }
    }
}
