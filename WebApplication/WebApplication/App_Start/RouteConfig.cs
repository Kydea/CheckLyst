using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace WebApplication
{
	public class RouteConfig
	{
		public static void RegisterRoutes(RouteCollection Routes)
		{
            Routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            Routes.MapMvcAttributeRoutes();
        }
    }
}
