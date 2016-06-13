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
using System.Data;
using Google.Apis.Drive.v2;
using System.Security.Cryptography.X509Certificates;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Drive.v2.Data;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Ichup.Controllers
{
    public class PhotosController : Controller
    {
        //
        // GET: /Photos/
        ichupEntities db = new ichupEntities();
        private string _fullPath_ = "";
        private string _fileName_ = "";
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
                string filePath = "";
                image img = db.images.Find(id);
                string link = img.link;
                string download = "total_download=total_download+1";
                if (type == 3) link = img.link;
                if (type == 2) { link = img.link_big; download = "total_download_big=total_download_big+1"; }
                if (type == 1) { link = img.link_small; download = "total_download_small=total_download_small+1"; }
                //link = img.link;
                string query = "update images set " + download + " where id=" + id;
                db.Database.ExecuteSqlCommand(query);
                //link=Config.domain + link;
                //System.Web.HttpContext.Current.Response.ContentType = "application/force-download";
                //System.Web.HttpContext.Current.Response.AddHeader("content-disposition", "attachment; filename=" + link);
                //System.Web.HttpContext.Current.Response.ContentType = "image/jpeg";
                //System.Web.HttpContext.Current.Response.TransmitFile(Server.MapPath(link));
                //System.Web.HttpContext.Current.Response.End();
                try
                {
                    if (type == 3)
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

                        filePath = HttpContext.Server.MapPath("../Images/Download/");//HttpContext.Server.MapPath(
                        downloadGDFile(service, link, filePath);
                    }
                    if (type == 3) filePath = "/Images/Download/";
                    System.Web.HttpContext.Current.Response.ContentType = "application/force-download";
                    System.Web.HttpContext.Current.Response.AddHeader("content-disposition", "attachment; filename=" + link+".jpg");
                    System.Web.HttpContext.Current.Response.ContentType = "image/jpeg";
                    System.Web.HttpContext.Current.Response.TransmitFile(filePath + link + ".jpg");
                    System.Web.HttpContext.Current.Response.End();
                    return "1";
                }
                catch (Exception ex)
                {
                    return "0";
                }
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
        public string UpdateFilter(int id, string filter_1, string filter_2, string filter_3, string filter_4, string filter_5, string keywords, int price, byte sale_type, string name, string address, float lon,float lat)
        {
            try
            {
                if (Config.getCookie("logged") == "") return "0";
                //string query = "update images set keywords=N'" + keywords + "',price=" + price + ",filter_1=N'" + filter_1 + "',filter_2=N'" + filter_2 + "',filter_3=N'" + filter_3 + "',filter_4=N'" + filter_4 + "',filter_5=N'" + filter_5 + "',sale_type=" + sale_type + ",name=N'" + name + "',address=N'" + address + "',lon=" + lon + ",lat=" + lat + " where id=" + id;
                //db.Database.ExecuteSqlCommand(query);
                image ig = db.images.Find(id);
                ig.keywords2 = keywords;
                ig.keywords = name + " " + keywords;
                ig.filter_1 = filter_1;
                ig.filter_2 = filter_2;
                ig.filter_3 = filter_3;
                ig.filter_4 = filter_4;
                ig.filter_5 = filter_5;
                ig.price = price;
                ig.sale_type = sale_type;
                ig.name = name;
                ig.address = address;
                ig.lat = lat;
                ig.lon = lon;
                ig.geocode = Config.CreatePoint(lat, lon);
                db.Entry(ig).State = EntityState.Modified;
                db.SaveChanges();
                return "1";
            }
            catch (Exception ex) { 

            }
            return "0";
        }
        SprightlySoftAWS.S3.CalculateHash MyCalculateHash;
        SprightlySoftAWS.S3.Upload MyUpload;
        System.ComponentModel.BackgroundWorker UploadBackgroundWorker;
        System.ComponentModel.BackgroundWorker CalculateHashBackgroundWorker;
        [HttpPost]
        [AcceptVerbs(HttpVerbs.Post)]
        public async Task<string> UploadImageProcess(HttpPostedFileBase file, bool autoname, bool free, int member_id)
        {
            if (Config.getCookie("logged") == "") return "0";
            string guid = Guid.NewGuid().ToString();
            string code = Config.genCode();
            string physicalPath = HttpContext.Server.MapPath("../" + Config.ImagePath + "\\");
            string nameFile = String.Format("{0}.jpg", guid + "-" + code);
            _fileName_ = nameFile;
            string nameFile1 = String.Format("{0}.jpg", guid+"-tb-small");
            string nameFile1_2 = String.Format("{0}.jpg", guid + "-small-"+Config.genCode());
            string nameFile2 = String.Format("{0}.jpg", guid + "-tb-big");
            string nameFile2_2 = String.Format("{0}.jpg", guid + "-big-" + Config.genCode());
            int countFile = Request.Files.Count;
            string fullPath = physicalPath + System.IO.Path.GetFileName(nameFile);
            string new_id = "";
            string basicname = "";
            string path1 = "";// resizeImage(1, fullPath, Config.ImagePath + "/" + nameFile1);//resize ảnh để hiển thị lúc tìm, ảnh nhỏ có wmark
            string path2 = "";// resizeImage(2, fullPath, Config.ImagePath + "/" + nameFile2);//resize ảnh để xem chi tiết ảnh và thông số ảnh, ảnh to có wmark
            string w1 = "";
            string w2 = "";

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
                Size size1 = new Size(Config.maxWidth1, Config.maxHeight1);
                Size size2 = new Size(Config.maxWidth3, Config.maxHeight3);
                ImageProcessor.ImageFactory iFF = new ImageProcessor.ImageFactory();
                //Tạo ra file thumbail không có watermark
                iFF.Load(fullPath).Resize(size1).BackgroundColor(Color.WhiteSmoke).Save(physicalPath + nameFile1_2);
                iFF.Load(fullPath).Resize(size2).BackgroundColor(Color.WhiteSmoke).Save(physicalPath + nameFile2_2);
                //Tạo ra file thumbail có watermark
                path1 = Config.ImagePath + "/" + nameFile1;// resizeImage(1, fullPath, Config.ImagePath + "/" + nameFile1);//resize ảnh để hiển thị lúc tìm, ảnh nhỏ có wmark
                path2 = Config.ImagePath + "/" + nameFile2;// resizeImage(2, fullPath, Config.ImagePath + "/" + nameFile2);//resize ảnh để xem chi tiết ảnh và thông số ảnh, ảnh to có wmark
                w1 = HttpContext.Server.MapPath("../" + path1);
                w2 = HttpContext.Server.MapPath("../" + path2);

                iFF.Load(physicalPath + nameFile1_2).Watermark(new TextLayer()
                {
                    DropShadow = true,
                    FontFamily = FontFamily.GenericMonospace,
                    Text = "BanAnhSo.Com",
                    Style = FontStyle.Regular,
                    FontSize = 12,
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
                iFF.Reset();
                iFF = null;
                var test = System.Drawing.Image.FromFile(fullPath);
                FileInfo f = new FileInfo(fullPath);
                _fullPath_ = fullPath;
                init();
                //Task<string> tsk = uploadGoogleDrive(fullPath);
                //string GGDRIVE_FILE_ID = tsk.Result;
                ////Link download https://drive.google.com/file/d/GGDRIVE_FILE_ID
                long size_file = f.Length;
                string filter_2 = "ngang";
                if (test.Height > test.Width) filter_2 = "dọc";
                string filter_1 = "ảnh";
                //Add vào db
                image img = new image();
                img.status = 0;
                img.keywords = basicname;
                img.link = nameFile;// GGDRIVE_FILE_ID;//Config.ImagePath + nameFile;
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
                f = null;
                break;
            }
            

            return path1 + ":" + new_id + ":" + basicname;// Config.ImagePath + "/" + nameFile;
        }
        public void init()
        {
            MyCalculateHash = new SprightlySoftAWS.S3.CalculateHash();
            //yCalculateHash.ProgressChangedEvent += MyCalculateHash_ProgressChangedEvent;

            MyUpload = new SprightlySoftAWS.S3.Upload();
            //MyUpload.ProgressChangedEvent += MyUpload_ProgressChangedEvent;

            CalculateHashBackgroundWorker = new System.ComponentModel.BackgroundWorker();
            CalculateHashBackgroundWorker.DoWork += CalculateHashBackgroundWorker_DoWork;
            CalculateHashBackgroundWorker.RunWorkerCompleted += CalculateHashBackgroundWorker_RunWorkerCompleted;

            UploadBackgroundWorker = new System.ComponentModel.BackgroundWorker();
            UploadBackgroundWorker.DoWork += UploadBackgroundWorker_DoWork;
            UploadBackgroundWorker.RunWorkerCompleted += UploadBackgroundWorker_RunWorkerCompleted;
            //Application.DoEvents();

            //Run the hash calculation in a BackgroundWorker process.  Calculating the hash of a
            //large file will take a while.  Running the process in a BackgroundWorker will prevent
            //the form from locking up.

            //Use a hash table to pass parameters to the function in the BackgroundWorker.
            Task task = new Task(ProcessDataAsync);
            task.Start();
            task.Wait();
        }
        public async void ProcessDataAsync()
        {
            // Start the HandleFile method.
            Task<string> task = ok();
            string x = await task;

        }
        public async Task<string> ok()
        {
            System.Collections.Hashtable CalculateHashHashTable = new System.Collections.Hashtable();
            CalculateHashHashTable.Add("LocalFileName", _fullPath_);
            CalculateHashBackgroundWorker.RunWorkerAsync(CalculateHashHashTable);
            return "ok";
        }
        private void CalculateHashBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            //Call the CalculateMD5FromFile function and set the result.  When the function is complete
            //the RunWorkerCompleted event will fire.  Use the LocalFileName value from the passed hash table.
            System.Collections.Hashtable CalculateHashHashTable = e.Argument as System.Collections.Hashtable;
            e.Result = MyCalculateHash.CalculateMD5FromFile(CalculateHashHashTable["LocalFileName"].ToString());
        }
        private void CalculateHashBackgroundWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            //If the CalculateMD5FromFile function was successful upload the file.
            if (MyCalculateHash.ErrorNumber == 0)
            {


                //Set the extra request headers to send with the upload
                Dictionary<String, String> ExtraRequestHeaders = new Dictionary<String, String>();

                ExtraRequestHeaders.Add("Content-Type", "image/jpeg");
                //ExtraRequestHeaders.Add("x-amz-acl", "public-read");


                //Use the MD5 hash that was calculated previously.
                ExtraRequestHeaders.Add("Content-MD5", e.Result.ToString());



                String RequestURL;
                RequestURL = MyUpload.BuildS3RequestURL(true, "s3.amazonaws.com", "bananhso", _fileName_, "");

                String RequestMethod = "PUT";

                ExtraRequestHeaders.Add("x-amz-date", DateTime.UtcNow.ToString("r"));

                String AuthorizationValue;
                AuthorizationValue = MyUpload.GetS3AuthorizationValue(RequestURL, RequestMethod, ExtraRequestHeaders, "AKIAIR2TUTKM6EM5Q6WQ", "Uc5myRRoncvKFGXrL9gzaK5YwHYh6OXAUqZal4Tu");
                ExtraRequestHeaders.Add("Authorization", AuthorizationValue);

                //Create a hash table of of parameters to sent to the upload function.
                System.Collections.Hashtable UploadHashTable = new System.Collections.Hashtable();
                UploadHashTable.Add("RequestURL", RequestURL);
                UploadHashTable.Add("RequestMethod", RequestMethod);
                UploadHashTable.Add("ExtraRequestHeaders", ExtraRequestHeaders);
                UploadHashTable.Add("LocalFileName", _fullPath_);

                //Run the UploadFile call in a BackgroundWorker to prevent the Window from freezing.
                UploadBackgroundWorker.RunWorkerAsync(UploadHashTable);
            }
            else
            {

            }
        }
        private void UploadBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            //Run the UploadFile call.
            System.Collections.Hashtable UploadHashTable = e.Argument as System.Collections.Hashtable;
            e.Result = MyUpload.UploadFile(UploadHashTable["RequestURL"].ToString(), UploadHashTable["RequestMethod"].ToString(), UploadHashTable["ExtraRequestHeaders"] as Dictionary<String, String>, UploadHashTable["LocalFileName"].ToString());
        }
        private void UploadBackgroundWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            //System.Diagnostics.Debug.Print("");
            //System.Diagnostics.Debug.Print(MyUpload.LogData);
            //System.Diagnostics.Debug.Print("");

            //EnableDisableEnd();

            if (Convert.ToBoolean(e.Result) == true)
            {
                //MessageBox.Show("Upload complete.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                try
                {

                    if (System.IO.File.Exists(_fullPath_))
                    {
                        System.IO.File.Delete(_fullPath_);
                    }
                }
                catch (Exception ex2) { }
            }
            else
            {
                //Show the error message.
                String ResponseMessage;

                if (MyUpload.ResponseString == "")
                {
                    ResponseMessage = MyUpload.ErrorDescription;
                }
                else
                {
                    System.Xml.XmlDocument XmlDoc = new System.Xml.XmlDocument();
                    XmlDoc.LoadXml(MyUpload.ResponseString);

                    System.Xml.XmlNode XmlNode;
                    XmlNode = XmlDoc.SelectSingleNode("/Error/Message");

                    ResponseMessage = XmlNode.InnerText;
                }

                //MessageBox.Show(ResponseMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public async Task<string> uploadGoogleDrive(string filename)
        {
            try
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

                string filePath = filename;//HttpContext.Server.MapPath(
                string fileId = UploadFile(service, filePath);
                InsertPermission(service, fileId, "bananhso.com@gmail.com", "user", "writer");//writer

                return fileId;
            }
            catch (Exception ex) {
                return "0";
            }
        }
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
            Google.Apis.Drive.v2.Data.File body = new Google.Apis.Drive.v2.Data.File();
            body.Title = System.IO.Path.GetFileName(filePath);
            body.Description = "this file is uploaded from server";
            body.MimeType = GetMimeType(filePath);
            body.Parents = new List<ParentReference>() { new ParentReference() { Id = "root" } };

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
        /// <summary>
        /// Download a file
        /// Documentation: https://developers.google.com/drive/v2/reference/files/get
        /// </summary>
        /// <param name="_service">a Valid authenticated DriveService</param>
        /// <param name="_fileResource">File resource of the file to download</param>
        /// <param name="_saveTo">location of where to save the file including the file name to save it as.</param>
        /// <returns></returns>
        public static Boolean downloadFileFromIdGoogleDrive(DriveService _service, Google.Apis.Drive.v2.Data.File _fileResource, string _saveTo)
        {

            if (!String.IsNullOrEmpty(_fileResource.DownloadUrl))
            {
                try
                {
                    
                    var x = _service.HttpClient.GetByteArrayAsync(_fileResource.DownloadUrl);
                    byte[] arrBytes = x.Result;
                    System.IO.File.WriteAllBytes(_saveTo, arrBytes);//_saveTo
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occurred: " + e.Message);
                    return false;
                }
            }
            else
            {
                // The file doesn't have any content stored on Drive.
                return false;
            }
        }
        public string downloadGDFile(DriveService service, string id_gd_file,string path)
        {
            //Google.Apis.Drive.v2.Data.File body = new Google.Apis.Drive.v2.Data.File();
            FilesResource.GetRequest getF = new FilesResource.GetRequest(service, id_gd_file);
            Google.Apis.Drive.v2.Data.File f = getF.Execute();
            bool downloadable = downloadFileFromIdGoogleDrive(service, f, path + id_gd_file+".jpg");
            ////body.DownloadUrl=
            //try
            //{
            //    FilesResource.GetRequest request = service.Files.Get(id_gd_file);//service.Files.Insert(body, stream, GetMimeType(filePath));
            //    request.Download(
            //    string file_id = request.ResponseBody.Id;

            //    return file_id;
            //}
            //catch (Exception e)
            //{
            //    //Console.WriteLine("An error occurred: " + e.Message);
            //    return null;
            //}
            return downloadable.ToString();
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
