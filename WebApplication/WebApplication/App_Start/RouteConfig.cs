using System.Web.Mvc;
using System.Web.Routing;

namespace CheckListManager.WebApplication
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Loader",
                "{*Url}",
                new { Controller = "Default", Action = "Loader" },
                new { HTTPMethod = new HttpMethodConstraint("GET") }
            );

            routes.MapMvcAttributeRoutes();
        }
    }
}
