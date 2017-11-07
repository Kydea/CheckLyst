using CheckListManager.WorkFlows.Api;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Startup))]

namespace CheckListManager.WorkFlows.Api
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
	        WebApiConfig.Setup(app);
        }
    }
}
