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
namespace Ichup.Controllers
{
    public class AWSController : Controller
    {
        //
        // GET: /AWS/
        SprightlySoftAWS.S3.CalculateHash MyCalculateHash;
        SprightlySoftAWS.S3.Upload MyUpload;
        System.ComponentModel.BackgroundWorker UploadBackgroundWorker;
        System.ComponentModel.BackgroundWorker CalculateHashBackgroundWorker;
        public ActionResult Index()
        {
            //init();
            return View();
        }
        public void download()
        {
            
            string existingBucketName = "bananhso";// + _fileName_
            string keyName = "ae3b2cb3-58fd-4c75-ad40-ca6347868ffe-0b71b614.jpg";
            string keyId = "AKIAIR2TUTKM6EM5Q6WQ";
            string keySecret = "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";
            IAmazonS3 client = new AmazonS3Client(keyId, keySecret, Amazon.RegionEndpoint.USEast1);

                GetObjectRequest request = new GetObjectRequest();
                request.BucketName = existingBucketName;
                //request.EtagToMatch = "d1e217a67c10d2497f068f2895ec80f2";
                request.Key = keyName;
                request.ByteRange = new ByteRange(0, 10);
                //request.Key = keyName;
                //{
                //    BucketName = existingBucketName,
                //    Key = keyName
                //};

                using (GetObjectResponse response = client.GetObject(request))  
                {
                    string dest=HttpContext.Server.MapPath("../Images/Download/" + keyName);
                    response.WriteResponseStreamToFile(dest, false);
                    //string dest = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), keyName);
                    //if (!System.IO.File.Exists(dest))
                    //{
                    //    response.WriteResponseStreamToFile(dest);
                    //}
                    System.Web.HttpContext.Current.Response.Clear();
                    System.Web.HttpContext.Current.Response.AppendHeader("content-disposition", "attachment; filename=" + keyName + ".jpg");
                    System.Web.HttpContext.Current.Response.ContentType = "application/octet-stream";
                    System.Web.HttpContext.Current.Response.TransmitFile(dest);
                    System.Web.HttpContext.Current.Response.Flush();
                    System.Web.HttpContext.Current.Response.End();

                    // Clean up temporary file.
                    System.IO.File.Delete(dest);
                }
         
        }
        public void DownloadS3Object()
        {
            string awsBucketName="bananhso";
            string keyName = "AKIAIR2TUTKM6EM5Q6WQ";
            string keySecret = "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";
            using (var client = new AmazonS3Client(keyName, keySecret, Amazon.RegionEndpoint.USEast1))
            {
                Stream imageStream = new MemoryStream();
                //GetObjectRequest request = new GetObjectRequest { BucketName = awsBucketName, Key = keyName };
                GetObjectRequest request = new GetObjectRequest();
                request.BucketName = awsBucketName;
                request.EtagToMatch = "59e8f4559df78be3d68855c7a80ebbec";
                request.ByteRange = new ByteRange(0, 10);
                request.Key = "AKIAIR2TUTKM6EM5Q6WQ";
                //client.GetObject(request);
                using (GetObjectResponse response = client.GetObject(request))
                {
                    response.ResponseStream.CopyTo(imageStream);
                }
                imageStream.Position = 0;
                //save
                string dest = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), keyName) + ".jpg";
                if (!System.IO.File.Exists(dest))
                {
                    using (Stream output = new FileStream(dest, FileMode.CreateNew))//+"mycat.jpg"
                    {
                        byte[] buffer = new byte[32 * 1024];
                        int read;

                        while ((read = imageStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            output.Write(buffer, 0, read);
                        }
                    }
                }
                
                // Clean up temporary file.
                // System.IO.File.Delete(dest);
                //return imageStream;
            }
        }
        SprightlySoftAWS.S3.Download MyDownload;
        System.ComponentModel.BackgroundWorker DownloadBackgroundWorker;
        string _fileName = "";
        string dest = "";
        string dest2 = "";
        private string notFound = "";
        public ActionResult getFile(string filename)
        {
            _fileName = filename;
            ViewBag.filename = filename;
            dest = HttpContext.Server.MapPath("../Images/Download/" + filename);
            dest2 = "../Images/Download/" + filename;
            ViewBag.dest = dest2;
            if (System.IO.File.Exists(dest)) return RedirectToAction("downloadstepfinal", "AWS", new { filename = _fileName, dest = dest2 });
            initDownload();
            //return RedirectToAction("downloadstepfinal", "AWS", new { filename = _fileName, dest = dest2 });
            return View();
        }
        public async Task initDownload()
        {
            MyDownload = new SprightlySoftAWS.S3.Download();
            MyDownload.ProgressChangedEvent += MyDownload_ProgressChangedEvent;

            DownloadBackgroundWorker = new System.ComponentModel.BackgroundWorker();
            DownloadBackgroundWorker.DoWork += DownloadBackgroundWorker_DoWork;
            DownloadBackgroundWorker.RunWorkerCompleted += DownloadBackgroundWorker_RunWorkerCompleted;
            Task task = new Task(ProcessDataAsync2);
            task.Start();
            task.Wait();
        }
        public async void ProcessDataAsync2()
        {
            // Start the HandleFile method.
            Task<string> task = ok2();
            string x = await task;

        }
        public async Task<string> ok2()
        {
            downloadFile(_fileName);
            return "ok";
        }
        public void downloadFile(string filename) {
            //dest = HttpContext.Server.MapPath("../Images/Download/" + filename);
            //if (System.IO.File.Exists(dest)) return;
            String RequestURL;
            RequestURL = MyDownload.BuildS3RequestURL(true, "s3.amazonaws.com", "bananhso", filename, "");
            String RequestMethod = "GET";
            Dictionary<String, String> ExtraRequestHeaders = new Dictionary<String, String>();
            ExtraRequestHeaders.Add("x-amz-date", DateTime.UtcNow.ToString("r"));
            //ExtraRequestHeaders.Add("x-amz-acl", "public-read");

            String AuthorizationValue;
            AuthorizationValue = MyDownload.GetS3AuthorizationValue(RequestURL, RequestMethod, ExtraRequestHeaders, "AKIAIR2TUTKM6EM5Q6WQ", "Uc5myRRoncvKFGXrL9gzaK5YwHYh6OXAUqZal4Tu");
            ExtraRequestHeaders.Add("Authorization", AuthorizationValue);
           
            //Use a hash table to pass parameters to the function in the BackgroundWorker.
            System.Collections.Hashtable DownloadHashTable = new System.Collections.Hashtable();
            DownloadHashTable.Add("RequestURL", RequestURL);
            DownloadHashTable.Add("RequestMethod", RequestMethod);
            DownloadHashTable.Add("ExtraRequestHeaders", ExtraRequestHeaders);
            DownloadHashTable.Add("LocalFileName", dest);


            //Run the DownloadFile function in a BackgroundWorker process.  Downloading a large
            //file may take a long time.  Running the process in a BackgroundWorker will prevent
            //the Window from locking up.
            DownloadBackgroundWorker.RunWorkerAsync(DownloadHashTable);
        }
        private void DownloadBackgroundWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            //System.Diagnostics.Debug.Print("");
            //System.Diagnostics.Debug.Print(MyDownload.LogData);
            //System.Diagnostics.Debug.Print("");

            //EnableDisableEnd();

            if (Convert.ToBoolean(e.Result) == true)
            {
                //MessageBox.Show("Download complete.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //RedirectToAction("AWS", "downloadstepfinal?_fileName=" + _fileName + "&dest=" + dest);
                //return RedirectToAction("downloadstepfinal", "AWS", new { filename = _fileName, dest = dest2 });
            }
            else
            {
                //Delete a partially downloaded file
                //if (System.IO.File.Exists(TextBoxDownloadFileName.Text) == true)
                //{
                //    System.IO.File.Delete(TextBoxDownloadFileName.Text);
                //}
                notFound = "-1";
                //Show the error message.
                String ResponseMessage;

                if (MyDownload.ResponseString == "")
                {
                    ResponseMessage = MyDownload.ErrorDescription;
                }
                else
                {
                    System.Xml.XmlDocument XmlDoc = new System.Xml.XmlDocument();
                    XmlDoc.LoadXml(MyDownload.ResponseString);

                    System.Xml.XmlNode XmlNode;
                    XmlNode = XmlDoc.SelectSingleNode("/Error/Message");

                    ResponseMessage = XmlNode.InnerText;
                }

                //MessageBox.Show(ResponseMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public ActionResult downloadstepfinal(string fileName, string dest)
        {
                System.Web.HttpContext.Current.Response.Clear();
                System.Web.HttpContext.Current.Response.AppendHeader("content-disposition", "attachment; filename=" + _fileName + ".jpg");
                System.Web.HttpContext.Current.Response.ContentType = "application/octet-stream";
                System.Web.HttpContext.Current.Response.TransmitFile(dest);
                System.Web.HttpContext.Current.Response.Flush();
                System.Web.HttpContext.Current.Response.End();
                return View();
        }

        private void DownloadBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            System.Collections.Hashtable DownloadHashTable = e.Argument as System.Collections.Hashtable;

            //Call the DownloadFile function and set the result.  When the function is complete
            //the RunWorkerCompleted event will fire.  Use the parameters that were passed in the 
            //hash table.
            e.Result = MyDownload.DownloadFile(DownloadHashTable["RequestURL"].ToString(), DownloadHashTable["RequestMethod"].ToString(), DownloadHashTable["ExtraRequestHeaders"] as Dictionary<String, String>, DownloadHashTable["LocalFileName"].ToString(), false);
        }
        private void MyDownload_ProgressChangedEvent()
        {
            //if (this.InvokeRequired == true)
            //{
            //    this.BeginInvoke(new MethodInvoker(delegate() { MyDownload_ProgressChangedEvent(); }));
            //}
            //else
            //{
            //    //Set the progress bar when the ProgressChangedEvent is fired.
            //    if (MyDownload.BytesTotal > 0)
            //    {
            //        decimal MyDecimal = (Convert.ToDecimal(MyDownload.BytesTransfered) / Convert.ToDecimal(MyDownload.BytesTotal)) * 100;
            //        ProgressBarTransfered.Value = Convert.ToInt32(MyDecimal);

            //        SprightlySoftAWS.S3.Helper MyHelper = new SprightlySoftAWS.S3.Helper();
            //        LabelBytesTransfered.Text = MyHelper.FormatByteSize(MyDownload.BytesTransfered) + " / " + MyHelper.FormatByteSize(MyDownload.BytesTotal);
            //    }
            //}
        }
        public string checkFile(string filename){
            dest = HttpContext.Server.MapPath("../Images/Download/" + filename);
            if (notFound == "-1") return "-1";
            if (System.IO.File.Exists(dest)) return "1"; else return "0";
        }
        public void init() {
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
            CalculateHashHashTable.Add("LocalFileName", "E:\\My\\AnhCuoi\\testok.jpg");
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
                RequestURL = MyUpload.BuildS3RequestURL(true, "s3.amazonaws.com", "bananhso", "testok.jpg", "");

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
                UploadHashTable.Add("LocalFileName", "E:\\My\\AnhCuoi\\testok.jpg");

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
    }
}
