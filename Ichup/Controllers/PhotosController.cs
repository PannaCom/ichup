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
        public ActionResult Upload() {
            return View();
        }
        [HttpPost]
        public string test(HttpPostedFileBase file)
        {
            return "21";
        }
        [HttpPost]
        [AcceptVerbs(HttpVerbs.Post)]
        public string UploadImageProcess(HttpPostedFileBase file)
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
                Request.Files[i].SaveAs(fullPath);
                var test = System.Drawing.Image.FromFile(fullPath);
                string filter_2 = "ngang";
                if (test.Height > test.Width) filter_2 = "dọc";
                test = null;
                //Add vào db
                image img = new image();
                img.status = 0;
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
            //Size size1 = new Size(Config.maxWidth1, Config.maxHeight1);
            //Size size2 = new Size(Config.maxWidth3, Config.maxHeight3);
            ImageProcessor.ImageFactory iFF=new ImageProcessor.ImageFactory();
            //iFF.Load(fullPath).Resize(size1).Save(physicalPath + nameFile1);
            //iFF.Load(fullPath).Resize(size2).Save(physicalPath + nameFile2);
            string path1 = resizeImage(1, fullPath, Config.ImagePath + "/" + nameFile1);//resize ảnh để hiển thị lúc tìm, ảnh nhỏ có wmark
            string path2 = resizeImage(2, fullPath, Config.ImagePath + "/" + nameFile2);//resize ảnh để xem chi tiết ảnh và thông số ảnh, ảnh to có wmark
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
