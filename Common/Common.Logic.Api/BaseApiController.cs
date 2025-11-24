using System.Linq;
using System.Web.Http;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Logic.Api.Filters;

namespace CheckListManager.Common.Logic.Api
{
    [Compression]
    public class BaseApiController : ApiController
	{
		private DefaultIdentity _identity;
		protected DefaultIdentity Identity => _identity ?? (_identity = User?.Identity as DefaultIdentity);
    }
}
