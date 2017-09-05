using System;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Owin;
using WebApplication.Models;

namespace WebApplication
{
    public class AuthorizationConfig
    {
        public static void Setup(IAppBuilder App)
        {
            App.Use<IntegratedAuthenticationMiddleware>(new IntegratedAuthenticationOptions(), new IntegratedAuthenticationService());
        }
    }
}