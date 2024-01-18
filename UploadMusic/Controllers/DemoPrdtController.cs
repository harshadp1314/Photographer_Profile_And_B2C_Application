using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Web;
using System.Web.Mvc;

namespace UploadMusic.Controllers
{
    public class DemoPrdtController : Controller
    {
        // GET: DemoPrdt
        public ActionResult eAlbum()
        {
            return View();
        }

        public ActionResult Photographeraprofile()
        {
            return View(); 
        }
    }
}