using System.Threading;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using CheckListManager.WebApplication;
using Microsoft.Owin;
using Microsoft.Owin.BuilderProperties;
using Owin;

[assembly: OwinStartup(typeof(Startup))]
namespace CheckListManager.WebApplication
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            OnStartup(app);

            var properties = new AppProperties(app.Properties);
            var token = properties.OnAppDisposing;
            if (token != CancellationToken.None)
                token.Register(OnShutdown);
        }

        private static void OnStartup(IAppBuilder app)
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthorizationConfig.Setup(app);
        }

        private static void OnShutdown()
        {
        }
    }
}
