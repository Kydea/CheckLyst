using System.Web.Mvc;
using System.Configuration;
using CheckListManager.Common.Core.Web.Security;

namespace CheckListManager.WebApplication.Code
{
	public abstract class DefaultViewPage : WebViewPage
	{
		private DefaultIdentity _identity;

		protected DefaultIdentity Identity
			=> _identity ?? (_identity = User?.Identity as DefaultIdentity);

        public string BaseUrl => ConfigurationManager.AppSettings["BaseUrl"];
        public string ReportModule => ConfigurationManager.AppSettings["ReportModule"];
	}

	public abstract class DefaultViewPage<T> : WebViewPage<T>
	{
		private DefaultIdentity _identity;

		protected DefaultIdentity Identity
			=> _identity ?? (_identity = User?.Identity as DefaultIdentity);

        public string BaseUrl => ConfigurationManager.AppSettings["BaseUrl"];
        public string ReportModule => ConfigurationManager.AppSettings["ReportModule"];
    }
}