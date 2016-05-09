﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ichup.Models;
using ImageProcessor.Imaging;
using ImageProcessor.Processors;
using PagedList;
using Newtonsoft.Json;
using System.IO;
namespace Ichup.Controllers
{
    public class PhotosController : Controller
    {
        //
        // GET: /Photos/
        ichupEntities db = new ichupEntities();
        public ActionResult Index()
        {
            return View();
        }
        public string Update(int totalitem)
        {
            try
            {
                if (Config.getCookie("logged") == "") return "0";
                for (int i = 0; i < totalitem; i++)
                {
                    string keyword = "";
                    string price = "";
                    string id = "";
                    if (Request.Form["keyword_" + i] != null)
                    {
                        keyword = Request.Form["keyword_" + i].ToString();
                    }
                    if (Request.Form["price_" + i] != null)
                    {
                        price = Request.Form["price_" + i].ToString();
                    }
                    if (Request.Form["id_" + i] != null)
                    {
                        id = Request.Form["id_" + i].ToString();
                    }
                    int to_price = int.Parse(price);
                    keyword += "," + Config.getCookie("logged");
                    string query="update images set keywords=N'"+keyword+"',price="+to_price+" where id="+id;
                    db.Database.ExecuteSqlCommand(query);                    
                }
                return "1";
            }
            catch (Exception ex) {
                return "0";
            }
            return "0";
        }
        public ActionResult User(string keyword,int? id,int? page) {
            if (Config.getCookie("userid") == "") return RedirectToAction("Login", "Members");
            else
            {
                //Nếu không phải là editor
                if (Config.getCookie("type") != "3" || Config.getCookie("type") != "4") id = int.Parse(Config.getCookie("userid"));
            }
            if (keyword == null) keyword = "";
            keyword = keyword.Replace("%20", " ");
            var p = (from q in db.images where q.keywords.Contains(keyword) && q.member_id == id select q).OrderByDescending(o => o.id);
            if (Config.getCookie("type") == "3" || Config.getCookie("type") == "4") p = (from q in db.images where q.keywords.Contains(keyword) select q).OrderByDescending(o => o.id);
            if (page == null) page = 1;
            ViewBag.page = page;
            ViewBag.keyword = keyword;
            ViewBag.id = id;
            if (id == null) return View();
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            var c = (from q in db.categories orderby q.name select q.name);
            ViewBag.acategory = JsonConvert.SerializeObject(c.ToList());
            return View(p.ToPagedList(pageNumber, pageSize));
        }
        public ActionResult Upload() {
            if (Config.getCookie("logged") == "") return RedirectToAction("Login", "Members");
            ViewBag.member_id = Config.getCookie("userid");
            return View();
        }
        public string getCategoryCk() {
            var p = (from q in db.categories orderby q.name select q.name).ToList();
            return JsonConvert.SerializeObject(p);
        }
        public ActionResult Page(int id) {
            image img = db.images.Find(id);
            return View(img);
        }
        public string getImage(int id) {
            try
            {
                var p = (from q in db.images where q.id == id && q.status == 0 select q);
                string query = "update images set total_views=total_views+1 where id=" + id;
                db.Database.ExecuteSqlCommand(query);
                return JsonConvert.SerializeObject(p.ToList());
            }
            catch (Exception ex) {
                return "0";
            }
        }
        public string downloadfile(int id,int type) {
            try
            {
                image img = db.images.Find(id);
                string link = img.link;
                string download = "total_download=total_download+1";
                if (type == 3) link = img.link;
                if (type == 2) { link = img.link_big; download = "total_download_big=total_download_big+1"; }
                if (type == 1) { link = img.link_small; download = "total_download_small=total_download_small+1"; }
                string query = "update images set " + download + " where id=" + id;
                db.Database.ExecuteSqlCommand(query);
                //link=Config.domain + link;
                System.Web.HttpContext.Current.Response.ContentType = "application/force-download";
                System.Web.HttpContext.Current.Response.AddHeader("content-disposition", "attachment; filename=" + link);
                System.Web.HttpContext.Current.Response.ContentType = "image/jpeg";
                System.Web.HttpContext.Current.Response.TransmitFile(Server.MapPath(link));
                System.Web.HttpContext.Current.Response.End();
            }
            catch (Exception ex) {
                return "0";
            }
            //System.Web.HttpContext.Current.Response.Flush();
            return "1";
        }
        [HttpPost]
        public string test(HttpPostedFileBase file)
        {
            return "21";
        }
        [HttpPost]
        public string UpdateFilter(int id, string filter_1,string filter_2,string filter_3,string filter_4,string filter_5,string keywords,int price,byte sale_type)
        {
            try
            {
                if (Config.getCookie("logged") == "") return "0";
                string query = "update images set keywords=N'" + keywords + "',price=" + price + ",filter_1=N'" + filter_1 + "',filter_2=N'" + filter_2 + "',filter_3=N'" + filter_3 + "',filter_4=N'" + filter_4 + "',filter_5=N'" + filter_5 + "',sale_type=" + sale_type + " where id=" + id;
                db.Database.ExecuteSqlCommand(query);
                return "1";
            }
            catch (Exception ex) { 

            }
            return "0";
        }
        [HttpPost]
        [AcceptVerbs(HttpVerbs.Post)]
        public string UploadImageProcess(HttpPostedFileBase file, bool autoname, bool free,int member_id)
        {
            if (Config.getCookie("logged") == "") return "0";
            string guid = Guid.NewGuid().ToString();
            string code = Config.genCode();
            string physicalPath = HttpContext.Server.MapPath("../" + Config.ImagePath + "\\");
            string nameFile = String.Format("{0}.jpg", guid + "-" + code);
            
            string nameFile1 = String.Format("{0}.jpg", guid+"-tb-small");
            string nameFile1_2 = String.Format("{0}.jpg", guid + "-small-"+Config.genCode());
            string nameFile2 = String.Format("{0}.jpg", guid + "-tb-big");
            string nameFile2_2 = String.Format("{0}.jpg", guid + "-big-" + Config.genCode());
            int countFile = Request.Files.Count;
            string fullPath = physicalPath + System.IO.Path.GetFileName(nameFile);
            string new_id = "";
            string basicname = "";
            for (int i = 0; i < countFile; i++)
            {
                if (!Config.IsImage(Request.Files[i])) return Config.ImagePath + "/invalidimage.png";
                //if (System.IO.File.Exists(fullPath))
                //{
                //    System.IO.File.Delete(fullPath);
                //}
                basicname = Request.Files[i].FileName;
                //basicname = Config.removeSpecialCharName(basicname);
                //if (!autoname) basicname = "";
                Request.Files[i].SaveAs(fullPath);
                var test = System.Drawing.Image.FromFile(fullPath);
                FileInfo f = new FileInfo(fullPath);
                long size_file = f.Length;
                string filter_2 = "ngang";
                if (test.Height > test.Width) filter_2 = "dọc";
                string filter_1 = "ảnh";
                //Add vào db
                image img = new image();
                img.status = 0;
                img.keywords = basicname;
                img.link = Config.ImagePath + nameFile;
                img.link_thumbail_big = Config.ImagePath + nameFile2;
                img.link_big = Config.ImagePath + nameFile2_2;
                img.link_thumbail_small = Config.ImagePath + nameFile1;
                img.link_small = Config.ImagePath + nameFile1_2;                
                img.token = guid;
                img.filter_2 = filter_2;
                img.filter_1 = filter_1;
                img.code = code;
                img.total_buy = 0;
                img.total_download = 0;
                img.total_views = 0;
                img.date_post = DateTime.Now;
                img.member_id = member_id;
                img.price = 1000000;
                img.size = size_file;
                if (free) img.price = 0;
                img.stt = i;
                img.width = test.Width;
                img.height = test.Height;
                img.sale_type = 0;
                db.images.Add(img);
                db.SaveChanges();
                new_id = img.id.ToString();
                test = null;                
                break;
            }
            Size size1 = new Size(Config.maxWidth1, Config.maxHeight1);
            Size size2 = new Size(Config.maxWidth3, Config.maxHeight3);            
            ImageProcessor.ImageFactory iFF=new ImageProcessor.ImageFactory();
            //Tạo ra file thumbail không có watermark
            iFF.Load(fullPath).Resize(size1).BackgroundColor(Color.WhiteSmoke).Save(physicalPath + nameFile1_2);
            iFF.Load(fullPath).Resize(size2).BackgroundColor(Color.WhiteSmoke).Save(physicalPath + nameFile2_2);
            //Tạo ra file thumbail có watermark
            string path1 = Config.ImagePath + "/" + nameFile1;// resizeImage(1, fullPath, Config.ImagePath + "/" + nameFile1);//resize ảnh để hiển thị lúc tìm, ảnh nhỏ có wmark
            string path2 = Config.ImagePath + "/" + nameFile2;// resizeImage(2, fullPath, Config.ImagePath + "/" + nameFile2);//resize ảnh để xem chi tiết ảnh và thông số ảnh, ảnh to có wmark
            string w1 = HttpContext.Server.MapPath("../" + path1);
            string w2 = HttpContext.Server.MapPath("../" + path2);

            iFF.Load(physicalPath + nameFile1_2).Watermark(new TextLayer()
            {
                DropShadow = true,
                FontFamily = FontFamily.GenericMonospace,
                Text = "BanAnhSo.Com",
                Style = FontStyle.Regular,
                FontSize=12,
                FontColor = Color.WhiteSmoke
            }).Save(w1);
            iFF.Load(physicalPath + nameFile2_2).Watermark(new TextLayer()
            {
                DropShadow = true,
                FontFamily = FontFamily.GenericMonospace,
                Text = "BanAnhSo.Com",
                Style = FontStyle.Regular,
                FontSize = 12,
                FontColor = Color.WhiteSmoke
            }).Save(w2);
            
            iFF = null;
            return path1 + ":" + new_id + ":" + basicname;// Config.ImagePath + "/" + nameFile;
        }
        public string resizeImage(byte type,string fullPath, string path)
        {
            int maxWidth=Config.maxWidth1, maxHeight=Config.maxHeight1;
            
            var image = System.Drawing.Image.FromFile(fullPath);
            if (type == 1)//Nếu là resize ảnh hiển thị lúc tìm
            {
                //Nếu là ảnh dọc thì đảo chiều
                if (image.Height > image.Width)
                {
                    maxWidth = Config.maxWidth2;
                    maxHeight = Config.maxHeight2;
                }
            }
            else
            {//Nếu là resize ảnh hiển thị lúc xem chi tiết ảnh
                maxWidth = Config.maxWidth3;
                maxHeight = Config.maxHeight3;
                if (image.Height > image.Width)
                {
                    maxWidth = Config.maxWidth4;
                    maxHeight = Config.maxHeight4;
                }
            }
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);
            var newWidth = (int)(image.Width * ratioX);
            var newHeight = (int)(image.Height * ratioY);
            var newImage = new Bitmap(newWidth, newHeight);
            Graphics thumbGraph = Graphics.FromImage(newImage);

            thumbGraph.CompositingQuality = CompositingQuality.HighSpeed;//HighQuality
            thumbGraph.SmoothingMode = SmoothingMode.HighSpeed;
            //thumbGraph.InterpolationMode = InterpolationMode.HighQualityBicubic;

            thumbGraph.DrawImage(image, 0, 0, newWidth, newHeight);
            image.Dispose();

            string fileRelativePath = path;// "newsizeimages/" + maxWidth + Path.GetFileName(path);
            newImage.Save(HttpContext.Server.MapPath(fileRelativePath), newImage.RawFormat);
            return fileRelativePath;
        }
        public string Del(int id) {
            try
            {
                int userid = 0;
                if (Config.getCookie("userid") == "") return "0";
                else userid = int.Parse(Config.getCookie("userid"));
                image img = db.images.Find(id);
                if (img != null && (userid == img.member_id || Config.getCookie("type") == "3" || Config.getCookie("type") == "4"))
                {
                    string link1 = HttpContext.Server.MapPath("../" + img.link);
                    string link2 = HttpContext.Server.MapPath("../" + img.link_thumbail_small);
                    string link3 = HttpContext.Server.MapPath("../" + img.link_thumbail_big);
                    if (System.IO.File.Exists(link1))
                    {
                        System.IO.File.Delete(link1);                        
                    }
                    if (System.IO.File.Exists(link2))
                    {
                        System.IO.File.Delete(link2);
                    }
                    if (System.IO.File.Exists(link3))
                    {
                        System.IO.File.Delete(link3);
                    }
                    db.images.Remove(img);
                    db.SaveChanges();
                }
            }
            catch (Exception ex) {
                return "0";
            }
            return "1";
        }
    }
}
