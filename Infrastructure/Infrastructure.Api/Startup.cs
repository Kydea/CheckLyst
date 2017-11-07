using CheckListManager.Infrastructure.Api;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Startup))]

namespace CheckListManager.Infrastructure.Api
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            WebApiConfig.Setup(app);
        }
    }
}
