using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Matematici.Security
{
    public class DefaultIdentity : ClaimsIdentity
    {
        public static string Version => "1.0";

        public DefaultIdentity(IEnumerable<Claim> Claims) : base(Claims, DefaultAuthenticationTypes.ApplicationCookie)
        {
            AddClaim(new Claim(ClaimTypes.Version, Version));
        }

        public DefaultIdentity(ClaimsIdentity identity) : base(identity)
        {
            if (identity?.IsAuthenticated ?? false)
            {
                if (!int.TryParse(identity.FindFirst("Company")?.Value, out var Company))
                    throw new InvalidTokenException();
                if (identity.FindFirst(ClaimTypes.Version)?.Value != Version)
                    throw new InvalidTokenException();
                if (string.IsNullOrEmpty(Code))
                    throw new InvalidTokenException();

                this.Company = Company;
            }
        }

        public int Company { get; }

        public string Code => FindFirst(ClaimTypes.Sid)?.Value;
    }
}
