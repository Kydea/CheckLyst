using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace CheckListManager.Common.Core.Web.Security
{
	public class DefaultIdentity : ClaimsIdentity
	{
		public static string Version => "1.0";

		public DefaultIdentity(ClaimsIdentity identity) : base(identity)
		{
			if (!long.TryParse(identity.FindFirst("CompanyId")?.Value, out var companyId))
			{
				throw new InvalidTokenException();
			}
			if (identity.FindFirst(ClaimTypes.Version)?.Value != Version)
			{
				throw new InvalidTokenException();
			}
			if (string.IsNullOrEmpty(Code) || string.IsNullOrEmpty(Description))
			{
				throw new InvalidTokenException();
			}

			CompanyId = companyId;
			Roles = identity.FindAll(RoleClaimType)
				.Select(r => ParseRole(r.Value))
				.ToList();
		}

		public long CompanyId { get; }

		public string Code => FindFirst(ClaimTypes.Sid)?.Value;

		public IList<Role> Roles { get; }

		public string Description => Name;

		private static Role ParseRole(string value)
		{
			var roleTokens = value.Split('#');
			if (roleTokens.Length != 4)
				throw new InvalidTokenException();

			return new Role
			{
				Code = roleTokens[0] + '#' + roleTokens[1],
				PrototypeType = roleTokens[2] == string.Empty ? (int?)null : int.Parse(roleTokens[2]),
				Tag = roleTokens[3]
			};
		}
	}
}
