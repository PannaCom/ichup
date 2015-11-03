using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ichup.Models;
using System.Data;
using System.Security.Cryptography;
namespace Ichup.Controllers
{
    public class MembersController : Controller
    {
        //
        // GET: /Members/
        private ichupEntities db = new ichupEntities();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Register(int? id) {
            if (id == null) id = 0;
            ViewBag.id = id;
            return View();
        }
        [HttpPost]
        public string Update(string name,string pass,string email,string phone,string address,string passport,string captcha,int id) {
            MD5 md5Hash = MD5.Create();
            try
            {
                if (id == 0)
                {
                    if (Session["Captcha"] != null && Session["Captcha"].ToString() != captcha) {
                        return "0";
                    }
                    pass = Config.GetMd5Hash(md5Hash, pass);
                    member mb = new member();
                    mb.name = name;
                    mb.pass = pass;
                    mb.email = email;
                    mb.phone = phone;
                    mb.address = address;
                    mb.passport = passport;
                    mb.total_views = 0;
                    mb.date_reg = DateTime.Now;
                    db.members.Add(mb);
                    db.SaveChanges();
                    return mb.id.ToString();

                }
                else {
                    member mb = db.members.Find(id);
                    mb.name = name;
                    //mb.pass = pass;
                    mb.email = email;
                    mb.phone = phone;
                    mb.address = address;
                    mb.passport = passport;
                    db.Entry(mb).State = EntityState.Modified;
                    db.SaveChanges();
                    return id.ToString();
                }
            }
            catch (Exception ex) {
                return "0";
            }
            return "1";
        }
        // genarate captcha Image
        public ActionResult CaptchaImage(bool noisy = false)
        {
            string[] character = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L" };
            var rand = new Random((int)DateTime.Now.Ticks);
            //generate new question 
            int a1 = rand.Next(0, 9);
            int a2 = rand.Next(0, 9);
            int a3 = rand.Next(0, 9);
            string b1 = character[a1];
            string b2 = character[a2];
            string b3 = character[a3];

            string captcha = a1 + b1 + a2 + b2 + a3 + b3;

            //store answer 
            Session["Captcha"] = captcha;

            //image stream 
            FileContentResult img = null;

            using (var mem = new MemoryStream())
            using (var bmp = new Bitmap(130, 30))
            using (var gfx = Graphics.FromImage((Image)bmp))
            {
                gfx.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                gfx.SmoothingMode = SmoothingMode.AntiAlias;
                gfx.FillRectangle(Brushes.White, new Rectangle(0, 0, bmp.Width, bmp.Height));

                //add noise 
                if (noisy)
                {
                    int i, r, x, y;
                    var pen = new Pen(Color.Yellow);
                    for (i = 1; i < 10; i++)
                    {
                        pen.Color = Color.FromArgb(
                        (rand.Next(0, 255)),
                        (rand.Next(0, 255)),
                        (rand.Next(0, 255)));

                        r = rand.Next(0, (130 / 3));
                        x = rand.Next(0, 130);
                        y = rand.Next(0, 30);

                        gfx.DrawEllipse(pen, x - r, y - r, r, r);
                    }
                }

                //add question 
                gfx.DrawString(captcha, new Font("Tahoma", 15), Brushes.Gray, 2, 3);

                //render as Jpeg 
                bmp.Save(mem, System.Drawing.Imaging.ImageFormat.Jpeg);
                img = this.File(mem.GetBuffer(), "image/Jpeg");
            }

            return img;
        }
    }
}
