using System.Web.Mvc;
using CheckListManager.Common.Core.Web.Security;

namespace CheckListManager.WebApplication.Code
{
	public abstract class DefaultViewPage : WebViewPage
	{
		private DefaultIdentity _identity;

		protected DefaultIdentity Identity
			=> _identity ?? (_identity = User?.Identity as DefaultIdentity);
	}

	public abstract class DefaultViewPage<T> : WebViewPage<T>
	{
		private DefaultIdentity _identity;

		protected DefaultIdentity Identity
			=> _identity ?? (_identity = User?.Identity as DefaultIdentity);
	}
}