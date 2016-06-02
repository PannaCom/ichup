using Google.Apis.Drive.v2;
using System.Security.Cryptography.X509Certificates;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Drive.v2.Data;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace Ichup.Controllers
{
    public class GoogleController : Controller
    {
        private string GetMimeType(string fileName)
        {
            string mimeType = "application/unknown";
            string ext = System.IO.Path.GetExtension(fileName).ToLower();
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null)
                mimeType = regKey.GetValue("Content Type").ToString();
            return mimeType;
        }

        public string UploadFile(DriveService service, string filePath)
        {     
            File body = new File();
            body.Title = System.IO.Path.GetFileName(filePath);
            body.Description = "this file is uploaded from server";
            body.MimeType = GetMimeType(filePath);            
            body.Parents = new List<ParentReference>() { new ParentReference() { Id= "root" } };
                        
            byte[] byteArray = System.IO.File.ReadAllBytes(filePath);
            System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArray);
            try
            {
                FilesResource.InsertMediaUpload request = service.Files.Insert(body, stream, GetMimeType(filePath));
                request.Upload();
                string file_id = request.ResponseBody.Id;
                return file_id;                
            }
            catch (Exception e)
            {
                //Console.WriteLine("An error occurred: " + e.Message);
                return null;
            }
        }

        public Permission InsertPermission(DriveService service, String fileId, String who, String type, String role)
        {
            Permission newPermission = new Permission();
            newPermission.Value = who;
            newPermission.Type = type;
            newPermission.Role = role;
            try
            {
                return service.Permissions.Insert(newPermission, fileId).Execute();
            }
            catch (Exception e)
            {
                //Console.WriteLine("An error occurred: " + e.Message);
            }
            return null;
        }

        
        // GET: Google
        public ActionResult Index()
        {
            string[] scopes = new string[] { DriveService.Scope.Drive };
            string keyFilePath = HttpContext.Server.MapPath("~/GooglAPI-929de187bc0b.p12");
            var serviceAccountEmail = "googlapi@googlapi-1330.iam.gserviceaccount.com";//"googlapi@strong-return-132923.iam.gserviceaccount.com";
            var certificate = new X509Certificate2(keyFilePath, "notasecret", X509KeyStorageFlags.Exportable);//X509Certificate2(keyFilePath, "notasecret", X509KeyStorageFlags.Exportable);
            var credential = new ServiceAccountCredential(new ServiceAccountCredential.Initializer(serviceAccountEmail)
            {
                Scopes = scopes
            }.FromCertificate(certificate));

            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "GooglAPI",
            });

            string filePath = HttpContext.Server.MapPath("~/1999-eb696.jpg");
            string fileId = UploadFile(service, filePath);
            InsertPermission(service, fileId, "bananhso.com@gmail.com", "user", "writer");

            return View();
        }
    }
}