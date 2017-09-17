using CheckListManager.WebApplication.Models.IntegratedAuthentication;
using Owin;

namespace CheckListManager.WebApplication
{
    public class AuthorizationConfig
    {
        public static void Setup(IAppBuilder app)
        {
            app.Use<IntegratedAuthenticationMiddleware>(new IntegratedAuthenticationOptions(), new IntegratedAuthenticationService());
        }
    }
}