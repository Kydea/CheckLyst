using System.Linq;
using System.Web.Http;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Logic.Api.Messages;

namespace CheckListManager.Common.Logic.Api
{
	public class BaseApiController : ApiController
	{
		private DefaultIdentity _identity;

		protected DefaultIdentity Identity
			=> _identity ?? (_identity = User?.Identity as DefaultIdentity);

		protected IHttpActionResult Error(ResponseMessage message)
		{
			return new ErrorResult(Request, message);
		}

		protected bool ValidateUser(long? prototypeType = null, string tag = null)
		{
			if (!prototypeType.HasValue && tag == null)
			{
				return true;
			}

			return Identity.Roles.Any(r =>
				(prototypeType == null || r.PrototypeType == prototypeType.Value)
				&& (tag == null || r.Tag == tag)
			);
		}
	}
}
