using System.Web.Mvc;
using System.Web.Routing;

namespace Licenta
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Ruta care raspunde pentru LoadSubCategries din Product controller 
            // care va raspunde cu JSON
            routes.MapRoute(
               name: "GetAllSubcategoriesJson",
               url: "Product/New/LoadSubCategories/{catId}",
               defaults: new { controller = "Product", action = "LoadSubCategories", catId = UrlParameter.Optional }
           );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
