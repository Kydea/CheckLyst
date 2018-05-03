using CheckListManager.ExternalSources.Api;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Newtonsoft.Json.Serialization;
using Owin;
using System.Threading.Tasks;
using System.Web.Cors;
using System.Web.Http;

[assembly: OwinStartup(typeof(Startup))]

namespace CheckListManager.ExternalSources.Api
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var Configuration = new HttpConfiguration();
            Configuration.SuppressDefaultHostAuthentication();
            Configuration.MapHttpAttributeRoutes();
            Configuration.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            app.UseWebApi(Configuration);

            app.UseCors(new CorsOptions
            {
                PolicyProvider = new CorsPolicyProvider
                {
                    PolicyResolver = context => Task.FromResult(new CorsPolicy
                    {
                        AllowAnyOrigin = true,
                        AllowAnyHeader = true,
                        AllowAnyMethod = true,
                        PreflightMaxAge = 1000
                    })
                }
            });
        }
    }
}
