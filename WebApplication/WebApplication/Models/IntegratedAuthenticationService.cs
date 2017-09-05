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
    public class IntegratedAuthenticationService
    {
        public bool IsAuthorised(string UserCode, string Value)
        {
            return Value == "Test";
        }
    }
}
