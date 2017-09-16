using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication.IntegratedAuthentication
{
    public class IntegratedAuthenticationOptions : AuthenticationOptions
    {
        public IntegratedAuthenticationOptions() : base("checklist-manager-authentication")
        {
        }
    }
}