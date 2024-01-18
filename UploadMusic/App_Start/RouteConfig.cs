using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace UploadMusic
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            //routes.MapMvcAttributeRoutes();
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Login", action = "Index", id = UrlParameter.Optional }
            );

            #region Defining New Routes For Each Controller

           // routes.MapRoute(
           //    name: "blog/album_design_at_pixthon",
           //    url: "{id}",
           //    defaults: new { controller = "PhotographerProfile", action = "Index", id = UrlParameter.Optional }
           //);

            #endregion
        }
    }
}
