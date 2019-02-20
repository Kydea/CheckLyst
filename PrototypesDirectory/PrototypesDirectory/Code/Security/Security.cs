using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System;
using System.Security.Claims;
using System.Web;
using System.Linq;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.Net;

namespace Matematici.Security
{
    public class Security
    {
        public static string CurrentUser
        {
            get
            {
                if (HttpContext.Current?.User == null || HttpContext.Current.User.Identity == null || !HttpContext.Current.User.Identity.IsAuthenticated) return null;

                var Identity = GetIdentity();
                if (Identity == null) return null;

                return Identity.FindFirst(ClaimTypes.Sid).Value;
            }
        }

        private static readonly HashSet<string> AllTimeZoneIds = new HashSet<string>(TimeZoneInfo.GetSystemTimeZones().Select(tz => tz.Id));

        public static void Login(ViewModels.Membership.Login Model)
        {
            VerifyPassword(Model.Company, Model.Code, Model.Password);

            var Identity = new DefaultIdentity(new[] {
                new Claim(ClaimTypes.Sid, Model.Code),
                new Claim("Company", Model.Company.ToString())
            });
            SaveIdentity(Identity);
        }

        private static void VerifyPassword(int? Company, string Code, string Password)
        {
            if (!Company.HasValue)
                throw new Exception("Please enter a company");

            if (string.IsNullOrWhiteSpace(Code))
                throw new Exception("Please enter a user code");

            if (string.IsNullOrWhiteSpace(Password))
                throw new Exception("Please enter a password");

            try
            {
                var CompanySettings = Settings.Provider.Values.Companies[Company.Value];
                var LdapDirectoryIdentifier = new LdapDirectoryIdentifier(CompanySettings.LDAPServer, 636, true, false); //ldap: 389, ldaps: 636
                var NetworkCredential = new NetworkCredential((Code.Contains(@"\") ? string.Empty : @"eu\") + Code, Password);
                var LdapConnection = new LdapConnection(LdapDirectoryIdentifier, NetworkCredential, AuthType.Basic);

                LdapConnection.SessionOptions.SecureSocketLayer = true;
                LdapConnection.SessionOptions.ProtocolVersion = 3;
                if (Settings.Provider.Values.DevelopmentMode)
                    LdapConnection.SessionOptions.VerifyServerCertificate = new VerifyServerCertificateCallback((Connection, Certificate) => true); //not necessary on janssen server, use only for test for security reasons
                LdapConnection.Bind();

                DirectoryEntry Root = new DirectoryEntry($"LDAP://{CompanySettings.LDAPServer}", (Code.Contains(@"\") ? string.Empty : @"eu\") + Code, Password);
                DirectorySearcher Searcher = new DirectorySearcher(Root)
                {
                    Filter = $"(&(objectClass=user)(sAMAccountName={Code})(memberOf={CompanySettings.LDAPQuery}))"
                };
                if (Searcher.FindAll()?.Count <= 0)
                    throw new Exception($"User is not enabled");
            }
            catch (Exception Exception)
            {
                throw new Exception("LDAP Error. " + Exception.Message);
            }
        }

        private static ClaimsIdentity GetIdentity()
        {
            return (ClaimsIdentity)HttpContext.Current.User.Identity;
        }

        private static void SaveIdentity(ClaimsIdentity Identity)
        {
            var Autentication = HttpContext.Current.GetOwinContext().Authentication;
            Autentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            Autentication.SignIn(new AuthenticationProperties { IsPersistent = false, ExpiresUtc = DateTimeOffset.UtcNow.AddSeconds(Settings.Provider.Values.SessionTimeout + 120) }, Identity);
        }

        public static void Logout()
        {
            HttpContext.Current.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
        }
    }
}