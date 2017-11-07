using CheckListManager.Instances.Api;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Startup))]

namespace CheckListManager.Instances.Api
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
	        WebApiConfig.Setup(app);
        }
    }
}
