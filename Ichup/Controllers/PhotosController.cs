using System;
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
                    string query="update images set keywords=N'"+keyword+"',price="+to_price+" where id="+id;
                    db.Database.ExecuteSqlCommand(query);
                    return "1";
                }
            }
            catch (Exception ex) {
                return "0";
            }
            return "0";
        }
        public ActionResult User(int? id,int? page) {
            var p = (from q in db.images where q.member_id == id select q).OrderByDescending(o => o.id);
            if (page == null) page = 1;
            ViewBag.page = page;
            if (id == null) return View();
            int pageSize = 25;
            int pageNumber = (page ?? 1);
            var c = (from q in db.categories orderby q.name select q.name);
            ViewBag.acategory = JsonConvert.SerializeObject(c.ToList());
            return View(p.ToPagedList(pageNumber, pageSize));
        }
        public ActionResult Upload() {
            return View();
        }
        public string getCategoryCk() {
            var p = (from q in db.categories orderby q.name select q.name).ToList();
            return JsonConvert.SerializeObject(p);
        }
        [HttpPost]
        public string test(HttpPostedFileBase file)
        {
            return "21";
        }
        [HttpPost]
        public string UpdateFilter(int id, string filter_1,string filter_2,string filter_3,string filter_4,string filter_5,string keywords,int price)
        {
            try
            {
                string query = "update images set keywords=N'" + keywords + "',price=" + price + ",filter_1=N'"+filter_1+"',filter_2=N'"+filter_2+"',filter_3=N'"+filter_3+"',filter_4=N'"+filter_4+"',filter_5=N'"+filter_5+"' where id=" + id;
                db.Database.ExecuteSqlCommand(query);
                return "1";
            }
            catch (Exception ex) { 

            }
            return "0";
        }
        [HttpPost]
        [AcceptVerbs(HttpVerbs.Post)]
        public string UploadImageProcess(HttpPostedFileBase file,bool autoname)
        {
            string guid = Guid.NewGuid().ToString();
            string code = Config.genCode();
            string physicalPath = HttpContext.Server.MapPath("../" + Config.ImagePath + "\\");
            string nameFile = String.Format("{0}.jpg", guid + "-" + code);
            
            string nameFile1 = String.Format("{0}.jpg", guid+"-small");

            string nameFile2 = String.Format("{0}.jpg", guid + "-big");
            int countFile = Request.Files.Count;
            string fullPath = physicalPath + System.IO.Path.GetFileName(nameFile);
            string new_id = "";
            for (int i = 0; i < countFile; i++)
            {
                if (!Config.IsImage(Request.Files[i])) return Config.ImagePath + "/invalidimage.png";
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
                string basicname = Request.Files[i].FileName;
                basicname = Config.removeSpecialCharName(basicname);
                Request.Files[i].SaveAs(fullPath);
                var test = System.Drawing.Image.FromFile(fullPath);
                string filter_2 = "ngang";
                if (test.Height > test.Width) filter_2 = "dọc";
                test = null;
                //Add vào db
                image img = new image();
                img.status = 0;
                img.keywords = basicname;
                img.link = Config.ImagePath + nameFile;
                img.link_thumbail_big = Config.ImagePath + nameFile2;
                img.link_thumbail_small = Config.ImagePath + nameFile1;
                img.token = guid;
                img.filter_2 = filter_2;
                img.code = code;
                img.total_buy = 0;
                img.total_download = 0;
                img.total_views = 0;
                img.date_post = DateTime.Now;
                img.member_id = 1;
                img.price = 0;
                img.stt = i;
                db.images.Add(img);
                db.SaveChanges();
                new_id = img.id.ToString();
                break;
            }
            Size size1 = new Size(Config.maxWidth1, Config.maxHeight1);
            Size size2 = new Size(Config.maxWidth3, Config.maxHeight3);
            ImageProcessor.ImageFactory iFF=new ImageProcessor.ImageFactory();
            iFF.Load(fullPath).Resize(size1).Save(physicalPath + nameFile1);
            iFF.Load(fullPath).Resize(size2).Save(physicalPath + nameFile2);
            string path1 = Config.ImagePath + "/" + nameFile1;// resizeImage(1, fullPath, Config.ImagePath + "/" + nameFile1);//resize ảnh để hiển thị lúc tìm, ảnh nhỏ có wmark
            string path2 = Config.ImagePath + "/" + nameFile2;// resizeImage(2, fullPath, Config.ImagePath + "/" + nameFile2);//resize ảnh để xem chi tiết ảnh và thông số ảnh, ảnh to có wmark
            string w1 = HttpContext.Server.MapPath("../" + path1);
            string w2 = HttpContext.Server.MapPath("../" + path2);

            iFF.Load(w1).Watermark(new TextLayer()
            {
                DropShadow = true,
                FontFamily = FontFamily.GenericMonospace,
                Text = "BanAnhSo.Com",
                Style = FontStyle.Regular,
                FontSize=12,
                FontColor = Color.WhiteSmoke
            }).Save(w1);
            iFF.Load(w2).Watermark(new TextLayer()
            {
                DropShadow = true,
                FontFamily = FontFamily.GenericMonospace,
                Text = "BanAnhSo.Com",
                Style = FontStyle.Regular,
                FontSize = 12,
                FontColor = Color.WhiteSmoke
            }).Save(w2);
            iFF = null;
            return path1 + ":" + new_id;// Config.ImagePath + "/" + nameFile;
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
    }
}
