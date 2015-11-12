using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
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
        [HttpPost]
        public string test(string a) {
            return a;
        }
        [HttpPost]
        [AcceptVerbs(HttpVerbs.Post)]
        public string UploadImageProcess(HttpPostedFileBase file)
        {
            string guid = Guid.NewGuid().ToString();
            string physicalPath = HttpContext.Server.MapPath("../" + Config.ImagePath + "\\");
            string nameFile = String.Format("{0}.jpg", guid);
            int countFile = Request.Files.Count;
            string fullPath = physicalPath + System.IO.Path.GetFileName(nameFile);
            for (int i = 0; i < countFile; i++)
            {
                if (!Config.IsImage(Request.Files[i])) return Config.ImagePath + "/invalidimage.png";
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
                Request.Files[i].SaveAs(fullPath);
                //break;
            }
            //string ok = resizeImage(Config.imgWidthNews, Config.imgHeightNews, fullPath, Config.NewsImagePath + "/" + nameFile);
            return Config.ImagePath + "/" + nameFile;
        }
        public string resizeImage(int maxWidth, int maxHeight, string fullPath, string path)
        {

            var image = System.Drawing.Image.FromFile(fullPath);
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
