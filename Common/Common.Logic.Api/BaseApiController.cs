using System.Web.Http;
using CheckListManager.Common.Logic.Api.Authentication;
using CheckListManager.Common.Logic.Api.Messages;

namespace CheckListManager.Common.Logic.Api
{
	public class BaseApiController : ApiController
	{
		private DefaultIdentity _identity;

		public DefaultIdentity Identity
			=> _identity ?? (_identity = User?.Identity as DefaultIdentity);

		protected IHttpActionResult Error(ResponseMessage message)
		{
			return new ErrorResult(Request, message);
		}
	}
}
