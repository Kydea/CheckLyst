using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Infrastructure;
using Owin;

namespace CheckListManager.WebApplication
{
    public class AuthorizationConfig
    {
        // Per ulteriori informazioni sulla configurazione dell'autenticazione, visitare http://go.microsoft.com/fwlink/?LinkId=301864
        public static void Setup(IAppBuilder app)
        {
            // Consentire all'applicazione di utilizzare un cookie per memorizzare informazioni relative all'utente connesso
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Security/Login"),
				CookieSecure = CookieSecureOption.SameAsRequest,
            });
			
            // Use a cookie to temporarily store information about a user logging in with a third party login provider
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Rimuovere il commento dalle seguenti righe per abilitare l'accesso con provider di accesso di terze parti
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");

            //app.UseTwitterAuthentication(
            //   consumerKey: "",
            //   consumerSecret: "");

            //app.UseFacebookAuthentication(
            //   appId: "",
            //   appSecret: "");

            //app.UseGoogleAuthentication();
        }
    }
}