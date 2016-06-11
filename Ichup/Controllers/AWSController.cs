using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.Threading.Tasks;
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
            upload();
            return View();
        }
        public void upload() {
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
            ok();
           
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
                RequestURL = MyUpload.BuildS3RequestURL(true, "s3.amazonaws.com", "bananhso", "E:\\My\\AnhCuoi\\testok.jpg", "");

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
