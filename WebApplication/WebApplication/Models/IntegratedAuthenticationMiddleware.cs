using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication.Models
{
    public class IntegratedAuthenticationMiddleware : AuthenticationMiddleware<IntegratedAuthenticationOptions>
    {
        public IntegratedAuthenticationMiddleware(OwinMiddleware NextMiddleware, IntegratedAuthenticationOptions Options, IntegratedAuthenticationService Service) : base(NextMiddleware, Options)
        {
            this.Service = Service ?? throw new NullReferenceException();
        }

        public IntegratedAuthenticationService Service { get; }

        protected override AuthenticationHandler<IntegratedAuthenticationOptions> CreateHandler()
        {
            return new IntegratedAuthenticationHandler(Service);
        }
    }
}