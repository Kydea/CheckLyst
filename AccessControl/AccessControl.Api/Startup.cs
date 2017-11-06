using CheckListManager.AccessControl.Api;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Startup))]

namespace CheckListManager.AccessControl.Api
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
			WebApiConfig.Setup(app);
        }
    }
}
