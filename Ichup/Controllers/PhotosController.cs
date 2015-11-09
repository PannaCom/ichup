using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Ichup.Controllers
{
    public class PhotosController : Controller
    {
        //
        // GET: /Photos/

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Upload() {
            return View();
        }

    }
}
