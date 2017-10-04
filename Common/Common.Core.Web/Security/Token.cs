using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace CheckListManager.Common.Core.Web.Security
{
    public class Token
    {
        public static string Version => "1.0";

        public static User GetUser(ClaimsIdentity identity)
        {
            var versionClaims = identity.FindAll(ClaimTypes.Version).ToList();
            var codeClaims = identity.FindAll(ClaimTypes.Sid).ToList();
            var descriptionClaims = identity.FindAll(ClaimTypes.Name).ToList();
            var companyClaims = identity.FindAll("CompanyId").ToList();
            var roleClaims = identity.FindAll(ClaimTypes.Role).ToList();

            if (
                versionClaims.Count != 1 || codeClaims.Count != 1 || descriptionClaims.Count != 1 || companyClaims.Count != 1
                || versionClaims.Single().Value != Version
                || companyClaims.Single().ValueType != ClaimValueTypes.Integer
            )
                throw new InvalidTokenException();

            var rolesList = ParseRoleClaims(roleClaims);

            return new User
            {
                Code = codeClaims.Single().Value,
                Description = descriptionClaims.Single().Value,
                Company = int.Parse(companyClaims.Single().Value),
                Roles = rolesList
            };
        }

        private static IEnumerable<Role> ParseRoleClaims(IEnumerable<Claim> roleClaims)
        {
            foreach (var role in roleClaims)
            {
                var roleTokens = role.Value.Split('#');
                if (roleTokens.Length != 4)
                    throw new InvalidTokenException();

                yield return new Role
                {
                    Code = roleTokens[0] + '#' + roleTokens[1],
                    PrototypeType = roleTokens[2] == string.Empty ? (int?)null : int.Parse(roleTokens[2]),
                    Tag = roleTokens[3]
                };
            }

        }

    }
}
