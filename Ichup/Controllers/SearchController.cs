using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ichup.Models;
using Newtonsoft.Json;
using PagedList;
namespace Ichup.Controllers
{
    public class SearchController : Controller
    {
        //
        // GET: /Search/
        public class searchitem
        {
            public long id { get; set;}
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
        private ichupEntities db = new ichupEntities();
        public ActionResult Index(string k, string f1, string f2, string f3, string f4, string f5, string order, string to, int? pg)
        {
            if (k == null) k = "a";
            k = k.Replace("%20", " ");
            f1 = f1 != null ? f1 : "";f2 = f2 != null ? f2 : ""; f3 = f3 != null ? f3 : "";
            f4 = f4 != null ? f4 : ""; f5 = f5 != null ? f5 : "";
            f1 = f1.Replace("%20", " ");
            f2 = f2.Replace("%20", " ");
            f3 = f3.Replace("%20", " ");
            f4 = f4.Replace("%20", " ");
            f5 = f5.Replace("%20", " ");
            ViewBag.keyword = k;
            if (pg == null) pg = 1;
            string query=" SELECT top 1000 ";
            query += "FT_TBL.id,FT_TBL.link,FT_TBL.link_thumbail_small,FT_TBL.total_views,FT_TBL.keywords,FT_TBL.date_post,FT_TBL.filter_1,FT_TBL.filter_2,FT_TBL.filter_3,FT_TBL.filter_4,FT_TBL.filter_5,KEY_TBL.RANK FROM images AS FT_TBL INNER JOIN FREETEXTTABLE(images, keywords,'" + k + "') AS KEY_TBL ON FT_TBL.id = KEY_TBL.[KEY] ";
             query += " where (status=0) ";
             string[] item=new string[10];
            int i=0;
            string[] filter = new string[5]; filter[0] = f1; filter[1] = f2; filter[2] = f3; filter[3] = f4; filter[4] = f5;
            for (int f = 0; f < filter.Length; f++)
            {
                if (filter[f] != null)
                {
                    item = filter[f].Split(',');
                    if (item.Length >= 1 && item[0].Trim()!="") query += " and ((1=2) ";
                    int col = f + 1;
                    for (i = 0; i < item.Length; i++)
                        if (item[i] != "")
                        {
                            query += " or (filter_" + col + " like N'%" + item[i] + "%')";
                        }
                    if (item.Length >= 1 && item[0].Trim() != "")  query += " ) ";
                }
            }
            if (order == null) order = "RANK";
            query += " order by " + order;
            if (to == null) to="Desc";
            query += " "+to;
            ViewBag.f1 = f1;
            ViewBag.f2 = f2;
            ViewBag.f3 = f3;
            ViewBag.f4 = f4;
            ViewBag.f5 = f5;
            ViewBag.page = pg;
            ViewBag.order = order;
            ViewBag.to = to;
            var p = db.Database.SqlQuery<searchitem>(query);
            int pageSize = 8;
            int pageNumber = (pg ?? 1);
            return View(p.ToPagedList(pageNumber, pageSize));
        }

        //
        // GET: /Search/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /Search/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Search/Create

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Search/Edit/5

        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /Search/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Search/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Search/Delete/5

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
