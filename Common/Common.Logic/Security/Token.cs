using CheckListManager.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.Security
{
    public class Token
    {
        public static string Version => "1.0";

        public static ClaimsIdentity CraeateIdentity(string authenticationType, Users user)
        {
            var identity = new ClaimsIdentity(authenticationType);
            identity.AddClaim(new Claim(ClaimTypes.Version, Version));
            identity.AddClaim(new Claim(ClaimTypes.Sid, user.Code));
            identity.AddClaim(new Claim(ClaimTypes.Name, user.Description));
            identity.AddClaim(new Claim("CompanyId", user.Company.ToString(), ClaimValueTypes.Integer));

            foreach (var Function in user.UserFunctions)
                identity.AddClaim(new Claim(ClaimTypes.Role, $"{Function.FunctionGroup}#{Function.Function}#{Function.PrototypeType?.ToString() ?? string.Empty}#{Function.Tag ?? string.Empty}"));

            foreach (var Function in user.UserFunctions.Select(x => x.FunctionGroup).Distinct())
                identity.AddClaim(new Claim(ClaimTypes.Role, $"FunctionGroup#{Function}"));

            return identity;
        }

        public static Dictionary<string, string> CreateProperties(Users user)
        {
            return new Dictionary<string, string>
            {
                { "company", user.Company.ToString() },
                { "code", user.Code },
                { "name", user.Description } 
            };
        }

        public static User GetUser(ClaimsIdentity Identity)
        {
            var VersionClaims = Identity.FindAll(ClaimTypes.Version);
            var CodeClaims = Identity.FindAll(ClaimTypes.Sid);
            var DescriptionClaims = Identity.FindAll(ClaimTypes.Name);
            var CompanyClaims = Identity.FindAll("CompanyId");
            var RoleClaims = Identity.FindAll(ClaimTypes.Role);

            if (
                VersionClaims.Count() != 1 || CodeClaims.Count() != 1 || DescriptionClaims.Count() != 1 || CompanyClaims.Count() != 1
                || VersionClaims.Single().Value != Token.Version
                || CompanyClaims.Single().ValueType != ClaimValueTypes.Integer
            )
                throw new InvalidTokenException();

            var RolesList = ParseRoleClaims(RoleClaims);

            return new User
            {
                Code = CodeClaims.Single().Value,
                Description = DescriptionClaims.Single().Value,
                Company = int.Parse(CompanyClaims.Single().Value),
                Roles = RolesList
            };
        }

        private static IEnumerable<Role> ParseRoleClaims(IEnumerable<Claim> RoleClaims)
        {
            foreach (var Role in RoleClaims)
            {
                var RoleTokens = Role.Value.Split('#');
                if (RoleTokens.Count() != 4)
                    throw new InvalidTokenException();

                yield return new Role
                {
                    Code = RoleTokens[0] + '#' + RoleTokens[1],
                    PrototypeType = RoleTokens[2] == string.Empty ? (int?)null : int.Parse(RoleTokens[2]),
                    Tag = RoleTokens[3]
                };
            }

        }

    }
}
