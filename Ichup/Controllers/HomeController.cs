using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ichup.Models;
namespace Ichup.Controllers
{
    public class HomeController : Controller
    {
        private ichupEntities db = new ichupEntities();
        public ActionResult Index()
        {
            //ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";
            var p = (from q in db.images where q.status == 0 && q.price == 0 orderby q.total_views descending select q).OrderByDescending(o => o.total_views).Take(20).ToList();
            ViewBag.news = p;
            
            return View();
        }
        public ActionResult Price() {
            return View();
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult Register() {
            return View();
        }
    }
}
