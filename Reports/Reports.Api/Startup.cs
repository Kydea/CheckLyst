using CheckListManager.Reports.Api;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Startup))]

namespace CheckListManager.Reports.Api
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
			WebApiConfig.Setup(app);
        }
    }
}
