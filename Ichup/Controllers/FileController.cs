using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using System.IO;
using Ichup.Models;

namespace Ichup.Controllers
{
    public class FileController : Controller
    {
        //
        // GET: /File/
        private ichupEntities db = new ichupEntities();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult download(int id)
        {
            image f = db.images.Find(id);
            if (f == null)
            {
                return HttpNotFound();
            }
            string filename = f.link;
            string dest = HttpContext.Server.MapPath("../Images/Download/" + filename);

            //string dest = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), keyName);
            if (System.IO.File.Exists(dest))
            {
                System.Web.HttpContext.Current.Response.Clear();
                System.Web.HttpContext.Current.Response.AppendHeader("content-disposition", "attachment; filename=" + filename);
                System.Web.HttpContext.Current.Response.ContentType = "application/octet-stream";
                System.Web.HttpContext.Current.Response.TransmitFile("../Images/Download/" + filename);
                System.Web.HttpContext.Current.Response.Flush();
                System.Web.HttpContext.Current.Response.End();
                return View();
            }

            string existingBucketName = "bananhso";// + _fileName_
            string keyId = "AKIAIR2TUTKM6EM5Q6WQ";
            string keySecret = "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";
            IAmazonS3 client = new AmazonS3Client(keyId, keySecret, Amazon.RegionEndpoint.USEast1);

            GetObjectRequest request = new GetObjectRequest();
            request.BucketName = existingBucketName;
            //request.EtagToMatch = "d1e217a67c10d2497f068f2895ec80f2";
            request.Key = filename;

            //request.ByteRange = new ByteRange(0, 10);

            //{
            //    BucketName = existingBucketName,
            //    Key = keyName
            //};

            using (GetObjectResponse response = client.GetObject(request))
            {
                //Stream imageStream = new MemoryStream();
                //response.ResponseStream.CopyTo(imageStream);
                //if (!System.IO.File.Exists(dest))
                //{
                //    using (Stream output = new FileStream(dest, FileMode.CreateNew))//+"mycat.jpg"
                //    {
                //        byte[] buffer = new byte[32 * 1024];
                //        int read;

                //        while ((read = imageStream.Read(buffer, 0, buffer.Length)) > 0)
                //        {
                //            output.Write(buffer, 0, read);
                //        }
                //    }
                //}

                response.WriteResponseStreamToFile(dest, false);
                //string dest = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), keyName);
                //if (!System.IO.File.Exists(dest))
                //{
                //    response.WriteResponseStreamToFile(dest);
                //}
                System.Web.HttpContext.Current.Response.Clear();
                System.Web.HttpContext.Current.Response.AppendHeader("content-disposition", "attachment; filename=" + filename);
                System.Web.HttpContext.Current.Response.ContentType = "application/octet-stream";
                System.Web.HttpContext.Current.Response.TransmitFile(dest);
                System.Web.HttpContext.Current.Response.Flush();
                System.Web.HttpContext.Current.Response.End();
                // Clean up temporary file.
                //System.IO.File.Delete(dest);
            }
            return View();
        }
                  
                    
    }
}
