using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ichup.Models;
using PagedList;
using Newtonsoft.Json;
namespace Ichup.Controllers
{
    public class ImagesController : Controller
    {
        private ichupEntities db = new ichupEntities();

        //
        // GET: /Images/
        public class searchitem
        {
            public long id { get; set; }
            public string link { get; set; }
            public string link_thumbail_small { get; set; }
            public int total_views { get; set; }
            public string keywords { get; set; }
            public DateTime date_post { get; set; }
            public string filter_1 { get; set; }
            public string filter_2 { get; set; }
            public string filter_3 { get; set; }
            public string filter_4 { get; set; }
            public string filter_5 { get; set; }
            public int RANK { get; set; }

        }
        public ActionResult Index(string k,int? pg)
        {
            long id;
            bool isNum = long.TryParse(k, out id);
            if (k == null) k = "";
            //string query = " SELECT top 1000 ";
            //query += "FT_TBL.id,FT_TBL.link,FT_TBL.link_thumbail_small,FT_TBL.total_views,FT_TBL.keywords,FT_TBL.date_post,FT_TBL.filter_1,FT_TBL.filter_2,FT_TBL.filter_3,FT_TBL.filter_4,FT_TBL.filter_5,KEY_TBL.RANK FROM images AS FT_TBL INNER JOIN FREETEXTTABLE(images, keywords,'" + k + "') AS KEY_TBL ON FT_TBL.id = KEY_TBL.[KEY] ";
            //query += " where (status=0) order by id desc";
            //var p = db.Database.SqlQuery<searchitem>(query);
            var p = (from q in db.images where q.status == 0 && (q.keywords.Contains(k)|| q.id==id) select q).OrderByDescending(o => o.id).Take(100);
            ViewBag.k = k;
            int pageSize = 5;
            int pageNumber = (pg ?? 1);
            return View(p.ToPagedList(pageNumber, pageSize));
        }

        //
        // GET: /Images/Details/5

        public ActionResult Details(long id = 0)
        {
            image image = db.images.Find(id);
            if (image == null)
            {
                return HttpNotFound();
            }
            return View(image);
        }

        //
        // GET: /Images/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Images/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(image image)
        {
            if (ModelState.IsValid)
            {
                db.images.Add(image);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(image);
        }

        //
        // GET: /Images/Edit/5

        public ActionResult Edit(long id = 0)
        {
            image image = db.images.Find(id);
            if (image == null)
            {
                return HttpNotFound();
            }
            return View(image);
        }

        //
        // POST: /Images/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(image image)
        {
            if (ModelState.IsValid)
            {
                db.Entry(image).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(image);
        }

        //
        // GET: /Images/Delete/5

        public ActionResult Delete(long id = 0)
        {
            image image = db.images.Find(id);
            if (image == null)
            {
                return HttpNotFound();
            }
            return View(image);
        }

        //
        // POST: /Images/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            image image = db.images.Find(id);
            db.images.Remove(image);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}