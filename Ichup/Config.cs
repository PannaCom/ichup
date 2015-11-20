using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Ichup
{
    public class Config
    {
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
        public static string genCode()
        {
            Random rnd = new Random();
            string[] temp = new string[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };
            string[] temp2 = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            int a = rnd.Next(0, 26); // creates a number between 1 and 25
            int b = rnd.Next(0, 10); // creates a number between 1 and 9
            int c = rnd.Next(0, 26);
            int d = rnd.Next(0, 10);
            int e = rnd.Next(0, 26);
            string rs = temp[a] + temp2[b] + temp[c] + temp2[d] + temp[e];
            return rs;
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
    }
}