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
        public bool IsAuthorised(IHeaderDictionary RequestHeaders)
        {
            return true;

            //if (RequestHeaders.TryGetValue("x-authentication-token", out string[] Values) && Values?.Length == 1)
            //{
            //    string[] Tokens = Values[0].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            //    if (Service.IsAuthorised(Tokens[0], Tokens[1]))
            //        return true;
            //}

            //return false;
        }
    }
}
