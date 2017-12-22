using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace CheckListManager.Common.Core.Web.Security
{
	public class DefaultIdentity : ClaimsIdentity, IDefaultIdentity
    {
		public static string Version => "1.0";

		public DefaultIdentity(ClaimsIdentity identity) : base(identity)
		{
			if (!long.TryParse(identity.FindFirst("CompanyId")?.Value, out var companyId))
				throw new InvalidTokenException();
			if (identity.FindFirst(ClaimTypes.Version)?.Value != Version)
				throw new InvalidTokenException();
			if (string.IsNullOrEmpty(Code) || string.IsNullOrEmpty(Description))
				throw new InvalidTokenException();

			CompanyId = companyId;
			Roles = identity.FindAll(ClaimTypes.Role)
				.Select(r => ParseRole(r.Value))
				.ToList();
		}

		public long CompanyId { get; }

		public string Code => FindFirst(ClaimTypes.Sid)?.Value;

		public IList<Role> Roles { get; }

		public string Description => Name;

		public bool HasPermission(string functionGroup, string function = "", long? department = null, long? prototypeType = null, string tag = null)
		{
            if (string.IsNullOrWhiteSpace(functionGroup) || functionGroup.Contains('#')) throw new ArgumentException($"Invalid function group '{functionGroup}'");
			return Roles.Any(r =>
				r.Code.StartsWith(functionGroup + "#" + function)
                && r.Department == department
                && r.PrototypeType == prototypeType
				&& (tag == null || r.Tag == tag)
			);
		}

		private static Role ParseRole(string value)
		{
			var roleTokens = value.Split('#');
			if (roleTokens.Length < 2 || roleTokens.Length > 5)
                throw new InvalidTokenException();

            return new Role
			{
				Code = roleTokens[0] + '#' + roleTokens[1],
                Department = roleTokens.Length < 3 || roleTokens[2] == string.Empty ? (int?)null : int.Parse(roleTokens[2]),
                PrototypeType = roleTokens.Length < 4 || roleTokens[3] == string.Empty ? (int?)null : int.Parse(roleTokens[3]),
				Tag = roleTokens.Length < 5 ? null : roleTokens[4]
			};
        }
    }
}
