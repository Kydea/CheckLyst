using System.Web.Http;
using CheckListManager.Common.Logic.Authentication;

namespace CheckListManager.Common.Logic
{
	public class BaseApiController : ApiController
	{
		private DefaultIdentity _identity;

		public DefaultIdentity Identity
			=> _identity ?? (_identity = User?.Identity as DefaultIdentity);
	}
}
