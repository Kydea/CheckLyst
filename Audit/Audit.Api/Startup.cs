using CheckListManager.Audit.Api;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Startup))]

namespace CheckListManager.Audit.Api
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
        }
    }
}
