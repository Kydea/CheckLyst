using System.Security.Claims;

namespace CheckListManager.Common.Logic.Authentication
{
	public class DefaultIdentity : ClaimsIdentity
	{
		public DefaultIdentity(ClaimsIdentity identity) : base(identity)
		{
			if (long.TryParse(identity.FindFirst("CompanyId")?.Value, out var companyId))
			{
				CompanyId = companyId;
			}
		}

		public long CompanyId { get; }
	}
}
