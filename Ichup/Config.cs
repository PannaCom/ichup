﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Ichup.Models;
using System.Net;
using System.Net.Mail;
using System.Data.Spatial;
namespace Ichup
{
    public class Config
    {
        public static string domain = "http://ichup.binhyen.net/";//"http://localhost:53182/";//
        public static string fromEmail = "xe14.com@gmail.com";
        public static string fromEmailPass = "chanhniem";
        public static string ImagePath = "/Images/Upload/";
        public const int ImageMinimumBytes = 512;
        public static int maxWidth1 = 270;//for ảnh ngang lúc tìm
        public static int maxHeight1 = 180;
        public static int maxWidth2 = 150;//for ảnh dọc lúc tìm
        public static int maxHeight2 = 225;
        public static int maxWidth3 = 570;//for ảnh ngang lúc xem chi tiết
        public static int maxHeight3 = 363;
        public static int maxWidth4 = 363;//for ảnh dọc lúc xem chi tiết
        public static int maxHeight4 = 570;
        private static ichupEntities db = new ichupEntities();
        public static bool IsImage(HttpPostedFileBase postedFile)
        {
            //-------------------------------------------
            //  Check the image mime types
            //-------------------------------------------
            if (postedFile.ContentType.ToLower() != "image/jpg" &&
                        postedFile.ContentType.ToLower() != "image/jpeg" &&
                        postedFile.ContentType.ToLower() != "image/pjpeg" &&
                        postedFile.ContentType.ToLower() != "image/gif" &&
                        postedFile.ContentType.ToLower() != "image/x-png" &&
                        postedFile.ContentType.ToLower() != "image/png")
            {
                return false;
            }

            //-------------------------------------------
            //  Check the image extension
            //-------------------------------------------
            if (System.IO.Path.GetExtension(postedFile.FileName).ToLower() != ".jpg"
                && System.IO.Path.GetExtension(postedFile.FileName).ToLower() != ".png"
                && System.IO.Path.GetExtension(postedFile.FileName).ToLower() != ".gif"
                && System.IO.Path.GetExtension(postedFile.FileName).ToLower() != ".jpeg")
            {
                return false;
            }

            //-------------------------------------------
            //  Attempt to read the file and check the first bytes
            //-------------------------------------------
            try
            {
                if (!postedFile.InputStream.CanRead)
                {
                    return false;
                }

                if (postedFile.ContentLength < ImageMinimumBytes)
                {
                    return false;
                }

                byte[] buffer = new byte[512];
                postedFile.InputStream.Read(buffer, 0, 512);
                string content = System.Text.Encoding.UTF8.GetString(buffer);
                if (Regex.IsMatch(content, @"<script|<html|<head|<title|<body|<pre|<table|<a\s+href|<img|<plaintext|<cross\-domain\-policy",
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline))
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

            //-------------------------------------------
            //  Try to instantiate new Bitmap, if .NET will throw exception
            //  we can assume that it's not a valid image
            //-------------------------------------------

            try
            {
                using (var bitmap = new System.Drawing.Bitmap(postedFile.InputStream))
                {
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
        public static string getTypeUser(short? type){
            string val = "";
            switch (type) {
                case -1: val = "bị khóa"; break;
                case 0: val = "bán ảnh"; break;
                case 1: val = "mua ảnh"; break;
                case 2: val = "mua bán ảnh"; break;
                case 3: val = "editor"; break;
                case 4: val = "admin"; break;
            }
            return val;
        }
        public static string tags(string keyword) {
            string[] all = keyword.Split(',');
            string val = "";
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] != "") {
                    val += "<span class=\"label label-default\" style=\"cursor:pointer;\" onclick=\"removeFilter('" + all[i] + "');\">" + all[i] + " X</span>";
                }
            }
            return val;
        }
        public static string removeSpecialChar(string input)
        {
            input = input.Replace("-", "").Replace(":", "").Replace(",", "").Replace("_", "").Replace("'", "").Replace("\"", "").Replace(";", "").Replace("”", "").Replace(".", "").Replace("%", "").Replace("&", "");
            return input;
        }
        public static string removeSpecialCharName(string input)
        {
            input = input.Replace("-", " ").Replace(":", " ").Replace(",", " ").Replace("_", " ").Replace("'", " ").Replace("\"", " ").Replace(";", " ").Replace("”", " ").Replace(".", " ").Replace("%", " ").Replace("&", " ");
            return input;
        }
        public static string getF0CkUser(long id, string have)
        {
            if (have == null) have = "";
            string val = "";
            string schecked = "";
            
             if (have.Equals("0")) schecked = "checked"; else schecked = "";
             val += "<div class=\"item-checkbox\"><input type=\"checkbox\" id=\"f-" + id + "-0_1\" value=\"0\"  " + schecked + " class=\"css-checkbox\">Miễn phí</div>";
            
            return val;
        }
        public static string[] f0ck = new string[] { "0", "1000000"};
        public static string getF0Ck(string have)
        {
            if (have == null) have = "";
            have += "," + have+",";
            string val = "";
            string schecked = "";
            string display = "thương mại";
            for (int i = 0; i < f0ck.Length; i++)
            {
                if (have.Contains(","+f0ck[i]+",")) schecked = "checked"; else schecked = "";
                if (f0ck[i].Equals("0")) display = "miễn phí"; else display = "thương mại";
                val += "<div class=\"item-checkbox\"><input type=\"checkbox\" id=\"f-0_" + (i + 1) + "\" value=\"" + f0ck[i] + "\" onchange=\"search();\"  " + schecked + " class=\"css-checkbox\">" + display + "</div>";
            }
            return val;
        }
        public static string[] f1ck = new string[] { "ảnh", "vector", "illustrator"};
        public static string getF1CkUser(long id,string have)
        {
            if (have == null) have = "";
            string val = "";
            string schecked = "";
            for (int i = 0; i < f1ck.Length; i++)
            {
                if (have.Contains(f1ck[i])) schecked = "checked"; else schecked = "";
                val += "<div class=\"checkbox\"><input type=\"checkbox\" id=\"f-" + id + "-1_" + (i + 1) + "\" value=\"" + f1ck[i] + "\"  " + schecked + " class=\"css-checkbox\">" + f1ck[i]+"</div>";
            }
            return val;
        }
        public static string getF1Ck(string have)
        {
            if (have == null) have = "";
            string val = "";
            string schecked = "";
            for (int i = 0; i < f1ck.Length; i++)
            {
                if (have.Contains(f1ck[i])) schecked = "checked"; else schecked = "";
                val += "<div class=\"item-checkbox\"><input type=\"checkbox\" id=\"f-1_" + (i + 1) + "\" value=\"" + f1ck[i] + "\" onchange=\"search();\"  " + schecked + " class=\"css-checkbox\">" + f1ck[i]+"</div>";
            }
            return val;
        }
        public static string[] f2ck = new string[] { "dọc", "ngang", "rộng" };
        public static string getF2CkUser(long id,string have)
        {
            if (have == null) have = "";
            string val = "";
            string schecked = "";
            for (int i = 0; i < f2ck.Length; i++)
            {
                if (have.Contains(f2ck[i])) schecked = "checked"; else schecked = "";
                val += "<div class=\"checkbox\"><input type=\"checkbox\" id=\"f-" + id + "-2_" + (i + 1) + "\" value=\"" + f2ck[i] + "\"  " + schecked + ">" + f2ck[i] + "</div>";
            }
            return val;
        }
        public static string getF2Ck(string have)
        {
            if (have == null) have = "";
            string val = "";
            string schecked = "";
            for (int i = 0; i < f2ck.Length; i++)
            {
                if (have.Contains(f2ck[i])) schecked = "checked"; else schecked = "";
                val += "<div class=\"item-checkbox\"><input type=\"checkbox\" id=\"f-2_" + (i + 1) + "\" value=\"" + f2ck[i] + "\" onchange=\"search();\"  " + schecked + ">" + f2ck[i] + "</div>";
            }
            return val;
        }

        public static string getF3CkUser(long id,string have)
        {
            if (have == null) have = "";
            var p = (from q in db.categories orderby q.name select q.name).ToList();
            string val = "<table style=\"width:100%;\"><tr>";
            string schecked = "";

            for (int i = 0; i < p.Count; i++)
            {
                string name = p[i];
                if (have.Contains(name)) schecked = "checked"; else schecked = "";
                if (i % 4 == 0 && i>0) val += "</tr><tr>";
                val += "<td width=25%><div class=\"checkbox\"><input value='" + name + "' id=f-" + id + "-3_" + (i + 1) + " type=checkbox " + schecked + ">" + name + "</div></td>";
            }
            val += "</tr></table>";
            return val;
        }
        public static string getF3Ck(string have)
        {
            if (have == null) have = "";
            var p = (from q in db.categories orderby q.name select q.name).ToList();
            string val = "";
            string schecked = "";

            for (int i = 0; i < p.Count; i++)
            {
                string name = p[i];
                if (have.Contains(name)) schecked = "checked"; else schecked = "";
                val += "<div class=\"item-checkbox\"><input value='" + name + "' id=f-3_" + (i + 1) + " onchange=\"search();\" type=checkbox " + schecked + ">" + name + "</div>";
            }
            return val;
        }
        public static string[] f4ck = new string[] { "có người", "nam", "nữ", "gay", "les", "trẻ em", "thanh niên", "trung niên", "người già", "nước ngoài" };
        public static string getF4Ck(string have)
        {
            if (have == null) have = "";
            string val = "";
            string schecked = "";

            for (int i = 0; i < f4ck.Length; i++)
            {
                if (have.Contains(f4ck[i])) schecked = "checked"; else schecked = "";
                val += "<div class=\"item-checkbox\"><input type=\"checkbox\" id=\"f-4_" + (i + 1) + "\" value=\"" + f4ck[i] + "\" onchange=\"search();\" " + schecked + ">" + f4ck[i] + "</div>";
            }
            
            return val;
        }
        public static string getF4CkUser(long id,string have)
        {
            if (have == null) have = "";
            //string val = "";
            string schecked = "";
            string val = "<table style=\"width:100%;\"><tr>";
            for (int i = 0; i < f4ck.Length; i++)
            {
                if (have.Contains(f4ck[i])) schecked = "checked"; else schecked = "";
                if (i % 4 == 0 && i > 0) val += "</tr><tr>";
                val += "<td width=25%><div class=\"checkbox\"><input type=\"checkbox\" id=\"f-" + id + "-4_" + (i + 1) + "\" value=\"" + f4ck[i] + "\"  " + schecked + ">" + f4ck[i] + "</div></td>";
            }
            val += "</tr></table>";
            return val;
        }
        public static string[] f5ck = new string[] { "chân dung", "bán thân", "chụp 3/4", "toàn thân", "khoảnh khắc" };
        public static string getF5Ck(string have)
        {
            if (have == null) have = "";
            string val = "";
            string schecked = "";

            for (int i = 0; i < f5ck.Length; i++)
            {
                if (have.Contains(f5ck[i])) schecked = "checked"; else schecked = "";
                val += "<div class=\"item-checkbox\"><input type=\"checkbox\" id=\"f-5_" + (i + 1) + "\" value=\"" + f5ck[i] + "\" onchange=\"search();\" " + schecked + ">" + f5ck[i] + "</div>";
            }

            return val;
        }
        public static string getF5CkUser(long id,string have)
        {
            if (have == null) have = "";
            string val = "";
            string schecked = "";

            for (int i = 0; i < f5ck.Length; i++)
            {
                if (have.Contains(f5ck[i])) schecked = "checked"; else schecked = "";
                val += "<div class=\"checkbox\"><input type=\"checkbox\" id=\"f-" + id + "-5_" + (i + 1) + "\" value=\"" + f5ck[i] + "\"  " + schecked + ">" + f5ck[i] + "</div>"; 
            }

            return val;
        }
        public static string getSaleType(long id,byte? sale_type) {
            string val="";
            string schecked1="checked";
            string schecked2="";
            if (sale_type==1){
                schecked1="";
                schecked2="checked";
            }
            val = "<input type=\"radio\" name=\"sale_type_" + id + "\" id=\"sale_type_" + id + "_1\" value=\"0\" " + schecked1 + ">Bán nhiều lần<input type=\"radio\" name=\"sale_type_" + id + "\" id=\"sale_type_" + id + "_2\" value=\"1\" " + schecked2 + ">Bán độc quyền một lần";
            return val;
        }
        public static void setCookie(string field, string value)
        {
            HttpCookie MyCookie = new HttpCookie(field);
            MyCookie.Value = HttpUtility.UrlEncode(value);
            MyCookie.Expires = DateTime.Now.AddDays(365);
            HttpContext.Current.Response.Cookies.Add(MyCookie);
            //Response.Cookies.Add(MyCookie);           
        }
        public static string getCookie(string v)
        {
            try
            {
                return HttpUtility.UrlDecode(HttpContext.Current.Request.Cookies[v].Value.ToString());
            }
            catch (Exception ex)
            {
                return "";
            }
        }
        public static string getCategory()
        {
            try
            {
                var p = (from q in db.categories orderby q.name select q.name).ToList();
                string val = "";
                
                for (int i = 0; i < p.Count; i++)
                {
                    string name = p[i].ToUpperInvariant();
                    
                    val += "<li class=\"col-sm-4\"><a href=\"#\">"+name+"</a></li>";
                    //if (i % 2 == 0) val += "<br>";
                }
                return val;
            }
            catch (Exception ex) {
                return "";
            }
            
        }
        //public static string getCategorySearch(string have)
        //{
        //    var p = (from q in db.categories orderby q.name select q.name).ToList();
        //    string val = "";
        //    string schecked="";
        //    for (int i = 0; i < p.Count; i++)
        //    {
        //        string name = p[i];
        //        if (have!=null && have.Contains(name)) schecked = "checked"; else schecked = "";
        //        val += "<input value='" + name + "' id=f-3_" + (i + 1) + " type=checkbox onchange=\"search();\" " + schecked + ">" + name;
        //        //if (i % 2 == 0) val += "<br>";
        //    }
        //    return val;
        //}
        public static string genCode()
        {
            //Random rnd = new Random();
            //string[] temp = new string[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };
            //string[] temp2 = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            //int a = rnd.Next(0, 26); // creates a number between 1 and 25
            //int b = rnd.Next(0, 10); // creates a number between 1 and 9
            //int c = rnd.Next(0, 26);
            //int d = rnd.Next(0, 10);
            //int e = rnd.Next(0, 26);
            //string rs = temp[a] + temp2[b] + temp[c] + temp2[d] + temp[e];            
            return Guid.NewGuid().ToString().Split('-')[0];
        }
        public static string GetMd5Hash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash. 
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes 
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data  
            // and format each one as a hexadecimal string. 
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string. 
            return sBuilder.ToString();
        }
        public static DateTime convertDateTime(string date){
            try
            {
                DateTime date2 = DateTime.Now;
                DateTime.TryParse(date, out date2);
                return date2;
            }
            catch (Exception ex) {
                return DateTime.Now;
            }
        }
        public static string mail(string from, string to, string topic, string pass, string content)
        {
            try
            {
                var fromAddress = from;
                var toAddress = to;
                //Password of your gmail address
                string fromPassword = pass;
                // Passing the values and make a email formate to display
                string subject = topic;
                string body = content;
                //body += "Email: " + fromEmailAddress + "\n";
                //body += "Về việc: " + subject + "\n";
                //body += "Nội dung: \n" + content + "\n";
                // smtp settings
                MailMessage message = new MailMessage();
                message.From = new MailAddress(fromAddress);
                message.To.Add(toAddress);
                message.Subject = subject;
                message.IsBodyHtml = true;
                message.Body = body;
                var smtp = new System.Net.Mail.SmtpClient();
                {
                    smtp.Host = "smtp.gmail.com";//"smtp.gmail.com";
                    smtp.Port = 587;// 465;//587;
                    smtp.EnableSsl = true;
                    smtp.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                    smtp.Credentials = new NetworkCredential(fromAddress, fromPassword);
                    smtp.Timeout = 20000;
                }
                // Passing values to smtp object
                smtp.Send(message);
            }
            catch (Exception ex)
            {
                return "-1";
            }
            return "ok";
        }
        public static DbGeography CreatePoint(double? latitude, double? longitude)
        {
            if (latitude == null || longitude == null) return null;
            latitude = (double)latitude;
            longitude = (double)longitude;
            return DbGeography.FromText(String.Format("POINT({1} {0})", latitude, longitude));
        }
    }
}