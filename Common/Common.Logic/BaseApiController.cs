using System.Web.Http;
using CheckListManager.Common.Logic.Authentication;

namespace CheckListManager.Common.Logic
{
	public class BaseApiController : ApiController
	{
		private Identity _identity;

		public Identity Identity
			=> _identity ?? (_identity = User?.Identity as Identity);
	}
}
