using CheckListManager.DataSources.Api;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Startup))]

namespace CheckListManager.DataSources.Api
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
			WebApiConfig.Setup(app);
        }
    }
}
