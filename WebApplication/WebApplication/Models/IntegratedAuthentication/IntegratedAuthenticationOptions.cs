using Microsoft.Owin.Security;

namespace CheckListManager.WebApplication.Models.IntegratedAuthentication
{
    public class IntegratedAuthenticationOptions : AuthenticationOptions
    {
        public IntegratedAuthenticationOptions() : base("checklist-manager-authentication")
        {
        }
    }
}