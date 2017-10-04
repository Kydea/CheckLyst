using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using CheckListManager.Common.Core.Web.Security;

namespace CheckListManager.WebApplication
{
    public class Security
    {
        public static User CurrentUser
        {
            get
            {
                try
                {
                    var Identity = HttpContext.Current?.User?.Identity as ClaimsIdentity;
                    if (!HttpContext.Current.User.Identity.IsAuthenticated || Identity == null) return null;

                    return Token.GetUser(Identity);
                }
                catch
                {
                    Logout();
                    throw;
                }
            }
        }

        public static void Logout()
        {
            //TODO
        }
    }
}