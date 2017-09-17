using System;
using Microsoft.Owin;
using Microsoft.Owin.Security.Infrastructure;

namespace CheckListManager.WebApplication.Models.IntegratedAuthentication
{
    public class IntegratedAuthenticationMiddleware : AuthenticationMiddleware<IntegratedAuthenticationOptions>
    {
        public IntegratedAuthenticationMiddleware(OwinMiddleware nextMiddleware, IntegratedAuthenticationOptions options, IntegratedAuthenticationService service) : base(nextMiddleware, options)
        {
            Service = service ?? throw new NullReferenceException();
        }

        public IntegratedAuthenticationService Service { get; }

        protected override AuthenticationHandler<IntegratedAuthenticationOptions> CreateHandler()
        {
            return new IntegratedAuthenticationHandler(Service);
        }
    }
}