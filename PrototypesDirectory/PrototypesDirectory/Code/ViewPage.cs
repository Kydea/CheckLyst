using System.Linq;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Matematici.Controllers;
using System.Text;
using Matematici.Security;
using System.Security.Claims;

namespace Matematici
{
    public abstract class DefaultViewPage : WebViewPage
    {
        private DefaultIdentity _identity;

        protected DefaultIdentity Identity => _identity ?? (_identity = new DefaultIdentity(User?.Identity as ClaimsIdentity));

        public string BaseUrl => $"{Request.Url.Scheme}://{Request.Url.Authority}";
    }

    public abstract class DefaultViewPage<T> : WebViewPage<T>
    {
        private DefaultIdentity _identity;

        protected DefaultIdentity Identity => _identity ?? (_identity = new DefaultIdentity(User?.Identity as ClaimsIdentity));

        public string BaseUrl => $"{Request.Url.Scheme}://{Request.Url.Authority}";
    }
}
