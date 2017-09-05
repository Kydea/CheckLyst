using Microsoft.Owin;
using Microsoft.Owin.BuilderProperties;
using Owin;
using System.Threading;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

[assembly: OwinStartup(typeof(WebApplication.Startup))]
namespace WebApplication
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder App)
        {
            OnStartup(App);

            var Properties = new AppProperties(App.Properties);
            CancellationToken Token = Properties.OnAppDisposing;
            if (Token != CancellationToken.None)
                Token.Register(() => OnShutdown());
        }

        private void OnStartup(IAppBuilder App)
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthorizationConfig.Setup(App);
        }

        private void OnShutdown()
        {
        }
    }
}
